# Cartwheel roadmap

The curriculum runs about 12 weeks at 1 to 2 hours a day. Each task ends with a "Done when" check. Status: ✅ done, ▶️ next, ⬜ not started, ⏭️ skipped.

## Phase 0: Environment ✅

Angular CLI installed, SQL Server in Docker Compose with a named volume, git repo with README and
`.gitignore`, and the backend skeleton with an xUnit project.

## Phase 1: C# fundamentals through the domain ✅

1. ✅ `Category` and `Product` with validation, plus a console playground.
2. ✅ `Cart` and `CartItem` with stock rules, merging lines, and domain exceptions.
3. ✅ LINQ queries on the catalog and a deferred-execution experiment.
4. ✅ First xUnit tests for `Product` and `Cart`.
5. ✅ `IProductRepository`, the in-memory repository in Infrastructure, `CartService`, and a hand-written fake.

## Phase 2: ASP.NET Web API ✅

1. ✅ `Cartwheel.Api` and `Cartwheel.Shared` (.NET Standard 2.0). GET all and GET by id, DTOs,
   dependency injection lifetimes, a thread-safe singleton repository, Swagger UI.
2. ✅ POST, PUT, DELETE with data annotations and correct status codes. Category repository,
   categories endpoint, shared seed data, all-or-nothing `UpdateDetails`.
3. ✅ Search, category filter, in-stock filter, and sorting through the query string. Playground deleted.
4. ✅ Global exception handling that maps domain exceptions to ProblemDetails, so business errors
   return 4xx instead of 500. Consider exposing the cart through the API to have a real case.
5. ✅ Controller unit tests with a mocking library (Moq or NSubstitute).

## Phase 3: MSSQL and EF Core

1. ⏭️ Design the schema on paper, create it by hand in T-SQL, and write five queries by hand
   (join, count per category, price filter, and so on).
2. ✅ EF Core with the SQL Server provider, `CartwheelDbContext`, entity configurations, the first
   migration, and a comparison with the hand-made schema.
3. ✅ Replace the in-memory repositories with EF implementations behind the same interfaces.
4. ▶️ Pagination on the product list (page, page size, total count).
5. ⬜ Repository tests against SQLite or the EF in-memory provider, and the trade-offs of each.

Done when the API serves data from SQL Server and migrations run from the CLI.

## Phase 4: Angular fundamentals

1. ⬜ TypeScript warm-up: model Product and Cart and the total logic in plain TypeScript.
2. ⬜ Scaffold `frontend/cartwheel-web`, enable CORS on the API, show the product list.
3. ⬜ Product detail page with routing and route parameters.
4. ⬜ Search, category filter, and sort controls wired to the query parameters.
5. ⬜ Cart state in a service with signals, a cart page, a header badge, and `localStorage`.
6. ⬜ First Angular tests: a pipe, a service with `HttpClientTesting`, and a component.

## Phase 5: Orders and checkout

1. ⬜ Orders and OrderItems schema and migration. Hand-written T-SQL for revenue per month and the
   top 5 products by quantity sold.
2. ⬜ Checkout rules in the domain, run in one EF transaction. Order items snapshot the unit price.
3. ⬜ Checkout page with a reactive form, then confirmation and order history pages.
4. ⬜ Optimistic concurrency with a `RowVersion` on Product, handling two checkouts racing for stock.
5. ⬜ Checkout unit tests and the first integration test with `WebApplicationFactory`.

## Phase 6: Authentication and authorization

1. ⬜ ASP.NET Identity and JWT bearer authentication, register and login endpoints, protected orders.
2. ⬜ Seeded admin user and role-based policies. Only admins can change products.
3. ⬜ Angular login and register pages, an auth service, a token interceptor, and route guards.
4. ⬜ Integration tests for 401 and 403.

## Phase 7: Quality, structure, and the admin area

1. ⬜ Admin product management screens with reactive forms.
2. ⬜ Admin order list with status changes.
3. ⬜ Serilog, request logging, a health check endpoint, and per-environment settings, including the
   database host changing from `localhost` to the compose service name.
4. ⬜ Lazy-loaded admin feature, a global HTTP error interceptor, and a notification service.
5. ⬜ A full test pass on both sides.

## Phase 8: Ship it and prepare for the interview

1. ⬜ Dockerfiles for the API and the Angular app, with the whole stack in `docker-compose.yml`.
2. ⬜ GitHub Actions: build and test both sides on every push.
3. ⬜ README with an architecture diagram, how to run it, screenshots, and what was learned.
4. ⬜ Mock interview: 30 questions across the whole stack.

## Stretch ideas

Product images with file upload, reviews, caching with `IMemoryCache`, a background job for abandoned
carts, live stock updates with SignalR, and end-to-end tests with Playwright.
