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
