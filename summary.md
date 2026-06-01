# Project Architecture Summary

## 1. Project description and key features

This is a .NET 10 Web API for an order management system. It exposes CRUD endpoints for inventory items and customer orders, gated by role-based authorization on mutating operations. The stack is ASP.NET Core 10 on top of EF Core (SQLite), ASP.NET Identity for user and role management, and JWT bearer tokens for stateless authentication. SQLite keeps the project trivial to run locally — no external database required — while the architecture stays portable to PostgreSQL or SQL Server by swapping a single provider registration.

Key features:

- Full CRUD over the two domain resources: `InventoryItem` and `Order` (with nested items per order).
- Stateless JWT bearer authentication; tokens carry user id and role claims.
- Role-based authorization (`Admin`, `User`) centralized in a single `UserRoles` constant.
- Argon2id password hashing replacing Identity's default PBKDF2-HMAC-SHA256.
- Defense-in-depth on login: ASP.NET Identity lockout (5 attempts / 5 minutes) plus a per-IP fixed-window rate limiter (5 requests / minute, HTTP 429 on rejection).
- Global error envelope: every unhandled exception is converted to a uniform `{ message, path, errors, timeStamp }` JSON response.
- Feature-sliced architecture (controllers → services → repositories → EF Core) with dependency injection throughout.
- In-memory caching of list and detail reads, invalidated on writes.
- Interactive `dotnet run -- create-admin` CLI for bootstrapping the first admin without committing credentials.
- Automatic EF Core migration application on startup, plus seed data for roles and a sample inventory item.
- OpenAPI / Swagger UI available in the Development environment.

## 2. Major challenges and how I overcame them

These are the issues that surfaced during the code review pass and how each was resolved.

- **Broken JWT authentication.** Tokens were minted with null issuer and audience, but the validation parameters defaulted to requiring both, so every authenticated request failed. Resolved by explicitly setting `ValidateIssuer` and `ValidateAudience` to `false` in `Program.cs` and documenting that production deployments should set both ends to matching real values.
- **Silent JWT secret weakness.** A short `JWT_SECRET` (under 32 characters) would crash HMAC-SHA256 at first login rather than at startup. Added a length guard in `AppConfig` so the app fails fast with a clear message.
- **Order deletion crashed on FK constraint.** The `Order → InventoryItem` relationship uses `DeleteBehavior.Restrict`, but `DeleteAsync` was calling a plain `FindByIdAsync` + delete, raising a constraint violation for any order that already had items. Fixed by loading items via `FindWithItemsAsync` and clearing the collection before delete.
- **Error middleware swallowed failures after the response started.** The original middleware unconditionally rewrote the response, which throws when headers have already been flushed (streaming endpoints, etc.) and loses the original exception. Added a `Response.HasStarted` check that re-throws so ASP.NET's default writer handles it.
- **Inconsistent role strings.** Authorization attributes hardcoded the literal `"admin"`, easy to typo and to drift from the role actually seeded. Centralized to `UserRoles.Admin` everywhere.
- **Admin re-seed gate too strict.** The seeder skipped admin creation as soon as any user existed, so a deleted admin could never be re-seeded. Switched to a role-membership check via `GetUsersInRoleAsync(UserRoles.Admin)`.
- **Passwords echoed to the console.** The `create-admin` command used `Console.ReadLine()`, which displays the password as it is typed. Replaced with a `ReadPassword` helper using `Console.ReadKey(intercept: true)` that echoes `*` per keystroke and supports Backspace.
- **Redundant change-tracker writes.** `Update*` service methods were calling `_repository.Update(entity)` on entities already tracked from `FindByIdAsync`, which marks every column modified and risks "already-tracked" exceptions. Dropped the call — tracked entities persist automatically on `SaveAsync()`.
- **Cache-key fall-through produced misleading errors.** `GetKey(CacheKey, int)` silently fell through to the no-id overload for keys that don't accept an id, then threw a confusing message. Replaced with an explicit `ArgumentException` that names both overloads.
- **PBKDF2 weaker than current state of the art.** Identity's default is FIPS-compliant but not memory-hard, making GPU and ASIC attacks comparatively cheap. Swapped in an Argon2id implementation (`Common/Argon2idPasswordHasher.cs`) using OWASP-recommended parameters (m=20 MiB, t=2, p=1).
- **HTTP status code literals scattered across the code.** Hard to audit and easy to misuse. Replaced every literal with the `StatusCodes.Status...` constants from `Microsoft.AspNetCore.Http`.

## 3. Implementation of business logic, data persistence, and state management

**Business logic** lives in feature-sliced service classes under `Services/` (`Inventory`, `Orders`, `Auth`). Each slice contains an interface, an implementation, and the controller that calls it. Controllers are deliberately thin: they bind input, call a single service method, and translate the result into an HTTP response. The services own validation, caching, and DTO mapping. Command-style operations that do not fit the service-per-feature pattern (`CreateAdminUserCommand`) live under `Commands/`. All services are registered as scoped lifetimes so each HTTP request gets its own instance and its own `DbContext`.

**Data persistence** uses Entity Framework Core against a SQLite database. `AppDbContext` extends `IdentityDbContext<ApplicationUser>` so the Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) coexist with the domain tables (`InventoryItems`, `Orders`). Primary keys and validation constraints are configured in `OnModelCreating` via the Fluent API, and the `TimeStampMixin` is auto-managed by overriding `SaveChanges` / `SaveChangesAsync` so `CreatedAt` and `UpdatedAt` are set without service-layer code paths having to remember. Schema evolution is handled by EF Core migrations applied automatically at startup by `DbSeeder.SeedAsync`. The project ships two migrations:

1. `InitialCreate` — the core domain tables (`Orders`, `InventoryItems`).
2. `AddIdentity` — upgrades the context with Identity's tables.

A generic `IRepository<T>` abstraction sits between the services and `DbContext`. Repositories accept optional `Expression<Func<T, bool>>` filters and `Include` delegates so callers can compose queries without leaking `IQueryable` upward. Read-only queries pass `asNoTracking: true` to skip the change tracker; write paths intentionally use tracked queries so EF persists mutations on `SaveAsync()` without an explicit `Update` call.

**State management** is deliberately minimal. The API is stateless: there is no server-side session and no sticky-session requirement. Authentication state is carried in the JWT itself — identity and roles are encoded as claims, signed with HMAC-SHA256, and verified on each request. The only server-side mutable state is the in-memory cache for list and detail reads, scoped to the process and invalidated on writes (see §5). DI lifetimes reinforce the stateless model: repositories, services, and `DbContext` are scoped, so per-request state never leaks between requests; only `AppConfig` is registered as a singleton because it holds immutable environment-derived values.

## 4. Security measures

The API combines several overlapping layers so that a single misconfiguration does not compromise the system.

- **Stateless JWT bearer authentication.** Tokens are minted by `AuthService.LoginAsync` and signed with HMAC-SHA256. `ValidateIssuer` and `ValidateAudience` are explicitly turned off because this tutorial mints tokens without those fields; `ValidateLifetime` and `ValidateIssuerSigningKey` remain on. The `JWT_SECRET` length is checked at startup (≥32 characters) so a weak key fails fast.
- **Identity Core only.** Registering `.AddIdentityCore<ApplicationUser>()` instead of `.AddIdentity()` prevents the framework from silently adding cookie authentication as the default scheme, which would otherwise compete with the JWT setup.
- **Argon2id password hashing.** Identity's default PBKDF2-HMAC-SHA256 hasher is replaced via `services.Replace(...)` with `Argon2idPasswordHasher<ApplicationUser>` (in `Common/Argon2idPasswordHasher.cs`). The implementation uses OWASP-recommended parameters (m=20 MiB, t=2, p=1, 16-byte random salt, 32-byte hash) backed by the `Konscious.Security.Cryptography.Argon2` package.
- **Role-based authorization.** Mutating endpoints are guarded by `[Authorize]`; admin-only endpoints use `[Authorize(Roles = UserRoles.Admin)]`. Role strings live in a single constant to prevent typos and drift.
- **Identity lockout.** ASP.NET Identity is configured with `MaxFailedAccessAttempts = 5` and `DefaultLockoutTimeSpan = 5 min`. `SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)` increments the counter on every failed login; once locked, `AuthService` throws `ApiException` with `StatusCodes.Status423Locked`.
- **Per-IP login rate limiting.** The login action carries `[EnableRateLimiting("login")]`, which references a fixed-window policy (5 requests / minute / IP, queue limit 0). Rejected requests get HTTP 429 before the action even runs, blocking brute force at the network edge in addition to Identity's lockout.
- **Masked password input for `create-admin`.** Console-based admin creation reads the password with `Console.ReadKey(intercept: true)` so it never appears in the terminal or scroll-back.
- **Global error envelope.** `ErrorHandlingMiddleware` converts all unhandled exceptions into a uniform `{ message, path, errors, timeStamp }` JSON response. In non-development environments the 500-class message is replaced with a generic "Internal server error." string so internal details do not leak to clients.
- **Safe JWT claim construction.** Claim values are guarded with null-coalescing operators so a partially populated user does not crash token generation.
- **Secrets stay out of source control.** `JWT_SECRET`, `DB_CONNECTION_STRING`, and `ADMIN_*` are read from environment variables (loaded from a gitignored `.env` for local development). `appsettings.json` holds only non-sensitive framework defaults.

## 5. Caching and performance

Caching uses `IMemoryCache` with type-safe keys and TTLs defined in `CacheConstants` (an enum-driven helper in `Common/Constants.cs`). The `InventoryService` and `OrderService` look up the appropriate key via `GetKey(...)` and the TTL via `GetDuration(...)`, so there are no magic strings or scattered durations. Caches are invalidated on every create, update, and delete so list and detail reads never serve stale data; the inventory list cache is also invalidated by order mutations because order items affect inventory aggregates.

Other performance practices:

- **Projections at the edge.** The order list endpoint returns `OrderSummaryDto` (counts and totals) instead of eager-loading every item; full item detail is reserved for `GetById` and the `OrderDetailDto` shape. This keeps list payloads small and prevents accidental N+1 fanout.
- **No-tracking reads.** Repository methods accept an `asNoTracking: true` flag on read paths, which skips EF's change tracker and noticeably reduces memory and CPU usage for list endpoints.
- **Tracked-entity updates.** Write paths intentionally use tracked queries (`FindByIdAsync`, `FindWithItemsAsync`) so the change tracker persists property mutations on `SaveAsync()` without a redundant `Update(entity)` call — which would otherwise mark every column modified and slow the round-trip.
- **Scoped DI for per-request work.** Repositories, services, and `DbContext` are registered as scoped, so each request reuses one change tracker and one connection; `AppConfig` is registered as a singleton because its values are immutable.
- **Async all the way down.** Every database and Identity call is awaited (`*Async`), so threads are never blocked waiting for I/O.
