# Cartwheel: mentoring project

Cartwheel is a single-vendor online shop built as a learning project. Andrii is preparing for a
Full-Stack trainee program (.NET + Angular) that asks for C#, .NET Standard, .NET 6+, ASP.NET Web API,
Angular + TypeScript, basic MSSQL, and unit testing. Andrii started as a beginner in both stacks and
works about 1 to 2 hours a day.

## Working agreement

- **Claude is a mentor by default.** Explain concepts, propose the design, hand out numbered tasks with
  a "Done when" check, and review the work. Tasks are sized for one evening.
- **Write project code only when Andrii explicitly asks** (for example "fix the issues" or "add the rest").
  Then explain what changed and why, so it still teaches something.
- **Never commit or push.** Andrii does that. Remind Andrii when work is uncommitted.
- **No quiz questions.** Andrii asked to stop them at the start of Phase 3. Teach the "why" inside the
  task text and the review instead.
- **Reviews are verified, not assumed.** For every review:
  1. `git status` and `git log` to see what changed and whether it is committed or pushed.
  2. `dotnet build --no-incremental` from `backend/`. The bar is zero warnings.
  3. `dotnet test` from `backend/`.
  4. For API work, start the API in the background with the `http` launch profile
     (http://localhost:5189), call every endpoint and edge case with curl, then stop the server.
- **Only the user's latest request counts.** A task that isn't finished is reported as not finished.

## Progress

- Phase 0 (environment): done.
- Phase 1 (C# fundamentals through the domain): done.
- Phase 2 (ASP.NET Web API): done.
- Phase 3 (MSSQL and EF Core): Task 1 (hand-written schema and T-SQL) skipped at Andrii's request.
- Phase 3 Tasks 2 to 4 (EF Core model, `InitialCreate` migration, EF repositories, seeding, pagination): done.
- Phase 3 Task 5 (repository integration tests in Testcontainers) skipped for now at Andrii's request. The
  handout design: a separate `Cartwheel.IntegrationTests` project, one container per run through a
  collection fixture, the compose file's image, `MigrateAsync`, tables emptied per test, and a fresh
  `DbContext` per Arrange/Act/Assert step. A spike
  ran the same repository scenarios on three providers. EF in-memory: `ExecuteDeleteAsync` throws, search is
  case-sensitive, and the unique index isn't enforced. SQLite: sorting by `decimal` crashes the test host
  on this Mac's comma locale (`FormatException: '1499.0'`), and search is case-sensitive. SQL Server in a
  container passed every scenario, including the `DbUpdateConcurrencyException` branch; it starts in ~7 s.
- Phase 3: done (the API serves SQL Server data and migrations run from the CLI).
- Phase 4 Task 1 (TypeScript warm-up) skipped at Andrii's request. Teach its points in the Angular tasks:
  money in integer cents, union types, `as` is a claim and not a check, and types vanish at runtime.
- Phase 4 Task 2 (Angular app, CORS, product list): done. The repo moved to `practice/csharp/cartwheel`
  because a `#` in the path breaks Vite (`ng test` failed with `Cannot find module`, `ng serve` served a
  blank page). Keep the repo path free of `#`. **Next: Phase 4 Task 3 (product detail page with routing).**

Update this section after each approved task.

## Solution layout

- `docker-compose.yml` at the repo root runs SQL Server 2022 (amd64 image under Rosetta on Apple Silicon).
  The `sa` password comes from `.env`, which is git-ignored. `.env.example` documents it.
- `backend/Cartwheel.slnx`, targeting .NET 10 and C# 14:
  - `Cartwheel.Domain`: entities, business rules, domain exceptions, repository interfaces, `CartService`.
  - `Cartwheel.Infrastructure`: in-memory repositories, `SeedCatalog`, and EF Core in `Persistence/`:
    `CartwheelDbContext`, one `IEntityTypeConfiguration` per entity in `Configurations/`, and `Migrations/`.
    The API uses `EfProductRepository` and `EfCategoryRepository` (scoped). The in-memory repositories
    remain as alternative implementations with their own tests. `CartwheelSeeder` fills an empty database
    from `SeedCatalog` through `UseSeeding`/`UseAsyncSeeding` on `dotnet ef database update`.
  - `Cartwheel.Shared`: request and response DTOs. Targets `netstandard2.0` with `LangVersion latest`
    and an `IsExternalInit` stub so records and `init` work.
  - `Cartwheel.Api`: controllers, manual mapping extension methods in `Mapping/`, Swagger UI in
    development, and `Cartwheel.Api.http` as the manual test script (JetBrains syntax).
  - `Cartwheel.Tests`: xUnit tests, with hand-written fakes in `Fakes/` and controller tests in
    `Controllers/` (NSubstitute, with its analyzers).
- `backend/dotnet-tools.json` pins `dotnet-ef` as a local tool (`dotnet tool restore`). Migrations run from
  `backend/` with `--project Cartwheel.Infrastructure --startup-project Cartwheel.Api`.
- `frontend/cartwheel-web`: Angular 21.2 (standalone, zoneless, Vitest, SCSS, no SSR), generated with
  `--skip-git`. `ProductsApi` returns Observables; components hold state in signals. The API URL comes
  from `src/environments/`. Needs npm 11: npm 10.9.2 (bundled with Node 22.14) crashes installing
  Angular 21 with `Cannot read properties of null (reading 'edgesOut')`.
- CORS: the `Frontend` policy allows the origins in `Cors:AllowedOrigins` (`http://localhost:4200` in
  Development). `UseCors` sits after `UseExceptionHandler`, so error responses carry CORS headers too.

## Conventions settled so far

- **Domain objects protect themselves.** Private setters, validation in constructors and setters
  (C# 14 `field` keyword), methods named after events (`IncreaseStock`, `DecreaseStock`), and
  all-or-nothing updates (`Product.UpdateDetails` validates everything before changing anything).
- **Business errors** derive from `DomainException`. Argument problems use the `Argument*Exception` types.
- **Money** is `decimal`. Format it with an explicit culture. Validate ranges with
  `Range(typeof(decimal), ..., ParseLimitsInInvariantCulture = true)`, because this Mac uses a
  comma as the decimal separator.
- **Repositories:** interfaces live in the domain and implementations in Infrastructure. Lists come back
  materialized and read-only, always sorted with a tie-breaker. A missing item returns null, and
  remove or update returns a bool. Filtering happens in the repository, not the controller.
- **Errors:** `DomainExceptionHandler` (an `IExceptionHandler`) maps domain exceptions to ProblemDetails:
  insufficient stock and stock over `Product.MaxStockQuantity` are 409, not found is 404, and any other domain rule is 400. Unknown exceptions become a
  generic 500 with no details. `UseExceptionHandler()` is first in the pipeline. No `try/catch` in controllers.
- **API:** thin controllers, DTOs only (never domain objects), lowercase routes, `{id:guid}` constraints,
  201 with `CreatedAtAction` for creates, 204 for update and delete, 404 only when the resource in the
  URL doesn't exist, 400 through `ValidationProblem` for a bad id inside a body, and 200 with an empty
  list when a filter matches nothing.
- **Pagination:** `GET /api/products` returns `PagedResponse<T>` (`items`, `page`, `pageSize`, `totalCount`,
  `totalPages`). Offset paging, 1-based pages, default size 20, maximum 100, page capped at 1,000,000 so
  `(page - 1) * pageSize` can't overflow. The domain's `PageRequest` guards the same limits as the DTO's
  `[Range]`. A page past the end is 200 with no items and the real total. Categories are not paged.
- **Persistence:** mapping is Fluent API only, never attributes on domain classes. Keys use
  `ValueGeneratedNever()` because the domain creates ids. Constraints are named explicitly, the database
  repeats the domain's rules as check constraints, and relationships use `DeleteBehavior.Restrict`.
  EF-only private constructors suppress CS9264 locally with a comment. Product queries always
  `Include(p => p.Category)`. `GetByIdAsync` is tracked (the update flow depends on it), lists use
  `AsNoTracking()`, `UpdateAsync` only saves and turns `DbUpdateConcurrencyException` into `false`, and
  `RemoveAsync` uses `ExecuteDeleteAsync`. Queries use no `StringComparison` or `StringComparer`,
  because they don't translate to SQL; the case-insensitive collation handles it.
- **Configuration:** no secrets in committed files. `Program.cs` loads the repo-root `.env` with
  DotNetEnv, and `SqlConnectionStringBuilder` adds `MSSQL_SA_PASSWORD` to the password-less connection
  string in `appsettings.Development.json`.
- **Tests:** one test class per unit, names like `Method_Scenario_Expected`, a `CreateProduct` helper
  with defaults, Arrange/Act/Assert, and a check that failed operations leave state unchanged.

## Known open points

- `Cart` still throws `InvalidOperationException` for a product that isn't in the cart. Consider a
  domain exception when the cart gets an API.
- Enum query values also accept defined numbers (`sort=1`). Acceptable for now.
- `DomainExceptionHandler` maps exceptions by type only, so `ProductNotFoundException` is always 404,
  even when the id came from a body (which should be 400). Revisit when the cart gets an API.
- `EfProductRepository`'s `DbUpdateConcurrencyException` branch has no test, and the EF repositories
  have no tests at all, because Phase 3 Task 5 was skipped. The API no longer uses
  `InMemoryProductRepository` or `InMemoryCategoryRepository`; they survive only for their own tests.
- `Categories.Name` is unique in the database but not in the domain, so a duplicate would surface as a
  `DbUpdateException` (500) once categories can be created.
- Stock changes are check-then-act on a shared in-memory instance, so they are not safe under
  concurrent requests. Phase 5 Task 4 (optimistic concurrency) addresses this.

## Roadmap

@docs/ROADMAP.md
