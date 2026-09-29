# Cartwheel: mentoring project

Cartwheel is a single-vendor online shop built as a learning project. Andrii is preparing for a
Full-Stack trainee program (.NET + Angular) that asks for C#, .NET Standard, .NET 6+, ASP.NET Web API,
Angular + TypeScript, basic MSSQL, and unit testing. Andrii started as a beginner in both stacks and
works about 1 to 2 hours a day.

## Working agreement

- **Claude is a mentor by default.** Explain concepts, propose the design, hand out numbered tasks with
  a "Done when" check, review the work, and quiz. Tasks are sized for one evening.
- **Write project code only when Andrii explicitly asks** (for example "fix the issues" or "add the rest").
  Then explain what changed and why, so it still teaches something.
- **Never commit or push.** Andrii does that. Remind Andrii when work is uncommitted.
- **Every task ends with "Think about these" questions.** Grade the answers honestly at the next review:
  say what was right, what was wrong, and give the interview-ready answer.
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
- Phase 2 (ASP.NET Web API): Tasks 1 to 4 done. Task 4 quiz answers still to be graded.
- **Next: Phase 2, Task 5, controller unit tests with a mocking library.**

Update this section after each approved task.

## Solution layout

- `docker-compose.yml` at the repo root runs SQL Server 2022 (amd64 image under Rosetta on Apple Silicon).
  The `sa` password comes from `.env`, which is git-ignored. `.env.example` documents it.
- `backend/Cartwheel.slnx`, targeting .NET 10 and C# 14:
  - `Cartwheel.Domain`: entities, business rules, domain exceptions, repository interfaces, `CartService`.
  - `Cartwheel.Infrastructure`: in-memory repositories and `SeedCatalog`. EF Core arrives in Phase 3.
  - `Cartwheel.Shared`: request and response DTOs. Targets `netstandard2.0` with `LangVersion latest`
    and an `IsExternalInit` stub so records and `init` work.
  - `Cartwheel.Api`: controllers, manual mapping extension methods in `Mapping/`, Swagger UI in
    development, and `Cartwheel.Api.http` as the manual test script (JetBrains syntax).
  - `Cartwheel.Tests`: xUnit tests, with hand-written fakes in `Fakes/`.
- `frontend/` does not exist yet. Angular starts in Phase 4.

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
- **Tests:** one test class per unit, names like `Method_Scenario_Expected`, a `CreateProduct` helper
  with defaults, Arrange/Act/Assert, and a check that failed operations leave state unchanged.

## Known open points

- `Cart` still throws `InvalidOperationException` for a product that isn't in the cart. Consider a
  domain exception when the cart gets an API.
- Enum query values also accept defined numbers (`sort=1`). Acceptable for now.
- `DomainExceptionHandler` maps exceptions by type only, so `ProductNotFoundException` is always 404,
  even when the id came from a body (which should be 400). Revisit when the cart gets an API.
- Stock changes are check-then-act on a shared in-memory instance, so they are not safe under
  concurrent requests. Phase 5 Task 4 (optimistic concurrency) addresses this.

## Roadmap

@docs/ROADMAP.md
