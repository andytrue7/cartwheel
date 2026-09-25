# Cartwheel

Cartwheel is an online shop — an e-commerce storefront where customers can browse a product catalog, manage a cart, and place orders. It's currently a work in progress: the backend solution and database are scaffolded, and application features are being built out incrementally.

## Tech stack:
backend:
- C# 14, .Net 10
- MSSQL

frontend:
- Angular

## How to run

**Database**

The backend expects a SQL Server instance, provided via Docker Compose.

1. Copy `.env.example` to `.env` and set `MSSQL_SA_PASSWORD` to a password meeting SQL Server's complexity requirements.
2. Start the database:
   ```bash
   docker compose up -d
   ```
   This starts MSSQL on `localhost:1433`.

**Backend**

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
cd backend
dotnet build
dotnet test
```

To run api:
```bash
dotnet run --project Cartwheel.Api --launch-profile http
http://localhost:5189/swagger
```

**Frontend**

The Angular frontend hasn't been scaffolded yet — instructions will be added here once it exists.