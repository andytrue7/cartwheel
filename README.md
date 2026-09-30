# Cartwheel

Cartwheel is an online shop — an e-commerce storefront where customers can browse a product catalog, manage a cart, and place orders. It's currently a work in progress: the product catalog works end to end (SQL Server, ASP.NET Web API, and an Angular product list), and the cart, orders, and authentication are being built out incrementally.

## Tech stack:
backend:
- C# 14, .NET 10, ASP.NET Web API
- MSSQL with EF Core

frontend:
- Angular 21, TypeScript

## How to run

**Database**

The backend expects a SQL Server instance, provided via Docker Compose.

1. Copy `.env.example` to `.env` and set `MSSQL_SA_PASSWORD` to a password meeting SQL Server's complexity requirements.
2. Start the database:
   ```bash
   docker compose up -d
   ```
   This starts MSSQL on `localhost:1433`.
3. Create the schema and seed the catalog (from `backend/`):
   ```bash
   dotnet tool restore
   dotnet ef database update --project Cartwheel.Infrastructure --startup-project Cartwheel.Api
   ```

**Backend**

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
cd backend
dotnet build
dotnet test
```

To run the API (Swagger UI at http://localhost:5189/swagger):
```bash
dotnet run --project Cartwheel.Api --launch-profile http
```

**Frontend**

Requires Node.js with npm 11 or newer (npm 10 fails installing Angular 21). Keep the repository path free of
`#`, which breaks the Angular dev server.

```bash
cd frontend/cartwheel-web
npm install
npm start
```

The app runs at http://localhost:4200 and calls the API at http://localhost:5189, so start the API first.
To run the tests: `npm test`.
