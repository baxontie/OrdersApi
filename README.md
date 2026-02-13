# Orders API — AI Assisted Development Project

This project implements a production-style Orders Management REST API built with
ASP.NET Core (.NET 8) and PostgreSQL, accelerated using GitHub Copilot.

## Features

- Create orders (POST /api/orders)
- List orders with pagination
- Filtering by:
  - Status
  - Amount range
  - Date range
- PostgreSQL with EF Core migrations
- Automatic seed of 50 sample orders
- Swagger UI
- Integration tests
- 93% line coverage / 80% branch coverage

---

## API Documentation

### GET /api/orders

Retrieves a paginated list of orders with optional filtering capabilities.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `page` | integer | No | 1 | Page number (1-based) |
| `limit` | integer | No | 10 | Number of items per page (1-100) |
| `status` | enum | No | - | Filter by order status: `Pending`, `Paid`, `Shipped`, `Cancelled` |
| `minAmount` | decimal | No | - | Minimum order amount (inclusive) |
| `maxAmount` | decimal | No | - | Maximum order amount (inclusive) |
| `fromDate` | datetime | No | - | Filter orders created on or after this date (ISO 8601 format) |
| `toDate` | datetime | No | - | Filter orders created on or before this date (ISO 8601 format) |

#### Validation Rules

- **`page`**: Must be greater than or equal to 1
- **`limit`**: Must be between 1 and 100 (inclusive)
- **`minAmount`**: If both `minAmount` and `maxAmount` are provided, `minAmount` must be less than or equal to `maxAmount`
- **`fromDate`**: If both `fromDate` and `toDate` are provided, `fromDate` must be less than or equal to `toDate`
- **`status`**: Must be a valid `OrderStatus` enum value if provided

#### Response Structure

The response includes the paginated orders and metadata:

```json
{
  "items": [
    {
      "id": 1,
      "customerName": "John Doe",
      "status": "Paid",
      "amount": 150.50,
      "createdAt": "2026-02-10T10:30:00Z"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "limit": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

#### Response Metadata

- **`items`**: Array of order objects for the current page
- **`totalCount`**: Total number of orders matching the filters
- **`page`**: Current page number
- **`limit`**: Number of items per page
- **`totalPages`**: Total number of pages available
- **`hasNextPage`**: Boolean indicating if there are more pages after the current one
- **`hasPreviousPage`**: Boolean indicating if there are pages before the current one

#### Request Examples

**Basic pagination:**
```http
GET /api/orders?page=1&limit=10
```

**Filter by status:**
```http
GET /api/orders?status=Paid&page=1&limit=20
```

**Filter by amount range:**
```http
GET /api/orders?minAmount=100&maxAmount=500&page=1&limit=10
```

**Filter by date range:**
```http
GET /api/orders?fromDate=2026-02-01T00:00:00Z&toDate=2026-02-28T23:59:59Z
```

**Combined filters:**
```http
GET /api/orders?status=Paid&minAmount=100&maxAmount=1000&fromDate=2026-02-01T00:00:00Z&page=2&limit=15
```

#### Response Examples

**Success Response (200 OK):**
```json
{
  "items": [
    {
      "id": 1,
      "customerName": "John Doe",
      "status": "Paid",
      "amount": 150.50,
      "createdAt": "2026-02-10T10:30:00Z"
    },
    {
      "id": 2,
      "customerName": "Jane Smith",
      "status": "Paid",
      "amount": 275.00,
      "createdAt": "2026-02-09T14:20:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "limit": 10,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Error Response - Invalid page (400 Bad Request):**
```http
HTTP/1.1 400 Bad Request

Query parameter 'page' must be greater than or equal to 1.
```

**Error Response - Invalid limit (400 Bad Request):**
```http
HTTP/1.1 400 Bad Request

Query parameter 'limit' must be between 1 and 100.
```

**Error Response - Invalid amount range (400 Bad Request):**
```http
HTTP/1.1 400 Bad Request

Query parameter 'minAmount' must be less than or equal to 'maxAmount'.
```

**Error Response - Invalid date range (400 Bad Request):**
```http
HTTP/1.1 400 Bad Request

Query parameter 'fromDate' must be less than or equal to 'toDate'.
```

#### Notes

- Orders are sorted by `createdAt` in descending order (newest first)
- All date parameters should be provided in ISO 8601 format (e.g., `2026-02-10T10:30:00Z`)
- When no filters are applied, all orders are returned (paginated)
- Empty result sets return `items: []` with appropriate metadata

---

## Tech Stack

- ASP.NET Core Web API (.NET 8)
- PostgreSQL
- Entity Framework Core
- xUnit + FluentAssertions
- Coverlet + ReportGenerator
- GitHub Copilot

---

## Setup Instructions

### Clone repository

```bash
git clone https://github.com/baxontie/OrdersApi.git
cd OrdersApi
