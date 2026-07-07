# Task Management API

A .NET task management API with JWT authentication, Redis caching, refresh tokens, background job processing, and soft delete.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (local or Docker)
- [Redis](https://redis.io/download) (running on `localhost:6379`)

## Setup Instructions

1. **Clone the repository**

2. **Update the connection string** in `TaskManagment/appsettings.json` if your SQL Server instance differs from the default:

   ```json
   "DefaultConnection": "Server=localhost;Database=TaskManagmentDb;Trusted_Connection=true;TrustServerCertificate=true;"
   ```

3. **Apply EF Core migrations** to create the database:

   ```bash
   dotnet ef database update --project TaskManagment.Infrastructure --startup-project TaskManagment
   ```

4. **Ensure Redis is running** on `localhost:6379`. If using Docker:

   ```bash
   docker run -d -p 6379:6379 redis
   ```

## How to Run

```bash
dotnet run --project TaskManagment
```

The API will start on `https://localhost:5001` (or the port shown in the console). Swagger UI is available at `/swagger`.

On startup, the admin user is seeded automatically if it doesn't already exist.

## Seeded Admin Credentials

| Field    | Value              |
|----------|--------------------|
| Email    | admin@example.com  |
| Password | Admin@123          |
| Role     | Admin              |

Use these credentials to log in via `POST /api/auth/login` and receive a JWT token.

## Assumptions

- Redis is required and must be running on `localhost:6379` before starting the API.
- SQL Server is expected on `localhost` with Windows Authentication (`Trusted_Connection=true`). Adjust the connection string for SQL Authentication or Docker.
- The `Jwt:Key` in `appsettings.json` is a placeholder. Replace it with a secure key (at least 32 characters) in production.
- Logs are written to the `logging/` directory at the project root, rolling daily with a 30-day retention.
- Migrations must be applied manually via `dotnet ef database update` before the first run.
- Soft-deleted users cannot log in, but their email remains reserved and cannot be reused for new registrations.
