# Implementation Plan: Harden GET /api/orders Pagination & Filtering

## Overview
Transform GET /api/orders from clamping-based validation to strict HTTP 400 validation, add response metadata, and ensure production-ready behavior with comprehensive integration tests.

---

## File-Level Changes List

### Modified Files
1. **`Services/OrderService.cs`**
   - Update `PagedResult<T>` record to include `TotalPages`, `HasNextPage`, `HasPreviousPage`
   - Replace clamping logic with validation that throws exceptions
   - Add validation method for filter parameters

2. **`Controllers/OrdersController.cs`**
   - Add validation logic before calling service
   - Return HTTP 400 BadRequest for invalid inputs
   - Handle validation exceptions from service layer

3. **`README.md`**
   - Add validation rules section
   - Update response structure examples
   - Document error responses

### New Files
4. **`Dtos/PagedOrdersResponseDto.cs`** (optional - for explicit response shape)
   - Response DTO with metadata fields

5. **`Services/Validation/OrderQueryValidator.cs`** (optional - for cleaner separation)
   - Centralized validation logic

6. **`Tests/OrdersControllerTests.cs`** (if test project exists) OR
   **`../OrdersApi.Tests/Integration/OrdersControllerIntegrationTests.cs`**
   - 10-15 integration test cases

---

## Step-by-Step Implementation Plan

### Phase 1: Update Service Layer (OrderService.cs)

#### Step 1.1: Enhance PagedResult Record
- **Location**: `Services/OrderService.cs` line 46
- **Change**: Add computed properties to `PagedResult<T>`:
  ```csharp
  public sealed record PagedResult<T>(
      IEnumerable<T> Items, 
      int TotalCount, 
      int Page, 
      int Limit)
  {
      public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Limit);
      public bool HasNextPage => Page < TotalPages;
      public bool HasPreviousPage => Page > 1;
  }
  ```

#### Step 1.2: Create Validation Method
- **Location**: `Services/OrderService.cs` (new private method)
- **Change**: Add validation method that throws `ArgumentException`:
  ```csharp
  private void ValidateQueryParameters(
      int page, 
      int limit, 
      decimal? minAmount, 
      decimal? maxAmount, 
      DateTime? fromDate, 
      DateTime? toDate)
  {
      if (page < 1)
          throw new ArgumentException("Page must be >= 1", nameof(page));
      
      if (limit < 1 || limit > 100)
          throw new ArgumentException("Limit must be between 1 and 100", nameof(limit));
      
      if (minAmount.HasValue && maxAmount.HasValue && minAmount > maxAmount)
          throw new ArgumentException("minAmount cannot be greater than maxAmount", nameof(minAmount));
      
      if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
          throw new ArgumentException("fromDate cannot be greater than toDate", nameof(fromDate));
  }
  ```

#### Step 1.3: Update GetOrdersAsync Method
- **Location**: `Services/OrderService.cs` lines 54-103
- **Changes**:
  - Remove clamping logic (`Math.Max`, `Math.Clamp`)
  - Call `ValidateQueryParameters` at start of method
  - Keep EF Core query structure unchanged (AsNoTracking, filters before Count/Skip/Take)

---

### Phase 2: Update Controller Layer (OrdersController.cs)

#### Step 2.1: Add Validation in Controller
- **Location**: `Controllers/OrdersController.cs` lines 38-58
- **Change**: Wrap service call in try-catch to handle `ArgumentException`:
  ```csharp
  [HttpGet]
  public async Task<IActionResult> GetAll(...)
  {
      try
      {
          var result = await _service.GetOrdersAsync(...);
          return Ok(result);
      }
      catch (ArgumentException ex)
      {
          return BadRequest(new { error = ex.Message, parameter = ex.ParamName });
      }
  }
  ```

#### Step 2.2: Alternative: Validate Before Service Call
- **Prefer**: Validate in controller using ModelState for better ASP.NET Core integration
- **Change**: Use `[FromQuery]` with validation attributes or manual validation

---

### Phase 3: Create Integration Tests

#### Step 3.1: Set Up Test Project (if needed)
- Check if `OrdersApi.Tests` project exists
- If not, create test project:
  ```bash
  dotnet new xunit -n OrdersApi.Tests
  dotnet add OrdersApi.Tests reference OrdersApi.csproj
  dotnet add OrdersApi.Tests package Microsoft.AspNetCore.Mvc.Testing
  dotnet add OrdersApi.Tests package FluentAssertions
  dotnet add OrdersApi.Tests package Npgsql.EntityFrameworkCore.PostgreSQL
  ```

#### Step 3.2: Create Test Base Class
- **File**: `Tests/Integration/IntegrationTestBase.cs` or similar
- **Purpose**: WebApplicationFactory setup, database seeding/cleanup

#### Step 3.3: Implement Test Cases
- **File**: `Tests/Integration/OrdersControllerIntegrationTests.cs`
- **Structure**: Use `IClassFixture<WebApplicationFactory<Program>>` pattern

---

### Phase 4: Update Documentation (README.md)

#### Step 4.1: Add Validation Rules Section
- Document all validation rules with HTTP 400 examples

#### Step 4.2: Update Response Structure
- Show new metadata fields (`totalPages`, `hasNextPage`, `hasPreviousPage`)

#### Step 4.3: Add Error Response Examples
- Show HTTP 400 responses for each validation failure

---

## Test Cases List (10-15 Integration Tests)

### Default Behavior Tests
1. **Test_GetAll_WithDefaults_ReturnsFirstPage**
   - Request: `GET /api/orders` (no params)
   - Assert: Page=1, Limit=10, TotalCount present, Items count <= 10

2. **Test_GetAll_WithCustomPageAndLimit_ReturnsCorrectPage**
   - Request: `GET /api/orders?page=2&limit=5`
   - Assert: Page=2, Limit=5, correct items returned

### Validation Tests (HTTP 400)
3. **Test_GetAll_WithPageLessThanOne_Returns400**
   - Request: `GET /api/orders?page=0`
   - Assert: Status 400, error message about page >= 1

4. **Test_GetAll_WithPageZero_Returns400**
   - Request: `GET /api/orders?page=0`
   - Assert: Status 400

5. **Test_GetAll_WithNegativePage_Returns400**
   - Request: `GET /api/orders?page=-1`
   - Assert: Status 400

6. **Test_GetAll_WithLimitLessThanOne_Returns400**
   - Request: `GET /api/orders?limit=0`
   - Assert: Status 400, error message about limit range

7. **Test_GetAll_WithLimitGreaterThan100_Returns400**
   - Request: `GET /api/orders?limit=101`
   - Assert: Status 400

8. **Test_GetAll_WithMinAmountGreaterThanMaxAmount_Returns400**
   - Request: `GET /api/orders?minAmount=100&maxAmount=50`
   - Assert: Status 400, error message about minAmount > maxAmount

9. **Test_GetAll_WithFromDateGreaterThanToDate_Returns400**
   - Request: `GET /api/orders?fromDate=2026-02-15&toDate=2026-02-10`
   - Assert: Status 400, error message about date range

### Filtering Tests
10. **Test_GetAll_WithStatusFilter_ReturnsFilteredResults**
    - Request: `GET /api/orders?status=Paid`
    - Assert: All items have Status=Paid

11. **Test_GetAll_WithAmountRange_ReturnsFilteredResults**
    - Request: `GET /api/orders?minAmount=50&maxAmount=200`
    - Assert: All amounts between 50-200

12. **Test_GetAll_WithDateRange_ReturnsFilteredResults**
    - Request: `GET /api/orders?fromDate=2026-02-01&toDate=2026-02-28`
    - Assert: All CreatedAt within range

13. **Test_GetAll_WithCombinedFilters_ReturnsFilteredResults**
    - Request: `GET /api/orders?status=Paid&minAmount=100&fromDate=2026-02-01`
    - Assert: All filters applied correctly

### Edge Cases & Metadata Tests
14. **Test_GetAll_WithEmptyResults_ReturnsCorrectMetadata**
    - Setup: Filter that returns 0 results
    - Request: `GET /api/orders?status=Cancelled&minAmount=999999`
    - Assert: Items=[], TotalCount=0, TotalPages=0, HasNextPage=false, HasPreviousPage=false

15. **Test_GetAll_OnLastPage_ReturnsCorrectMetadata**
    - Setup: TotalCount=25, Limit=10
    - Request: `GET /api/orders?page=3&limit=10`
    - Assert: Page=3, TotalPages=3, HasNextPage=false, HasPreviousPage=true

16. **Test_GetAll_OnFirstPage_ReturnsCorrectMetadata**
    - Request: `GET /api/orders?page=1&limit=10`
    - Assert: HasPreviousPage=false, HasNextPage based on total

17. **Test_GetAll_OnMiddlePage_ReturnsCorrectMetadata**
    - Setup: TotalCount=50, Limit=10
    - Request: `GET /api/orders?page=2&limit=10`
    - Assert: HasPreviousPage=true, HasNextPage=true

---

## Implementation Order

1. ✅ **Phase 1**: Update Service Layer (validation + metadata)
2. ✅ **Phase 2**: Update Controller Layer (error handling)
3. ✅ **Phase 3**: Create Integration Tests
4. ✅ **Phase 4**: Update README Documentation

---

## Key Implementation Notes

### EF Core Efficiency
- ✅ Keep `AsNoTracking()` for read-only queries
- ✅ Apply filters before `CountAsync()`
- ✅ Apply filters before `Skip()` and `Take()`
- ✅ Maintain existing indexes (Status, Amount, CreatedAt)

### Response Structure
```json
{
  "items": [...],
  "totalCount": 50,
  "page": 1,
  "limit": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Error Response Structure
```json
{
  "error": "Page must be >= 1",
  "parameter": "page"
}
```

---

## Validation Rules Summary

| Parameter | Rule | HTTP Status |
|-----------|------|-------------|
| `page` | Must be >= 1 | 400 |
| `limit` | Must be 1-100 (inclusive) | 400 |
| `minAmount` | Cannot be > `maxAmount` (if both provided) | 400 |
| `fromDate` | Cannot be > `toDate` (if both provided) | 400 |
| `status` | Must be valid enum value (if provided) | 400 (if invalid enum) |

---

## Testing Strategy

- Use `WebApplicationFactory<Program>` for integration tests
- Use in-memory PostgreSQL or test database
- Seed test data before each test or use shared fixture
- Clean up after tests (or use transactions)
- Test both success and failure paths
- Verify response structure matches expected schema
