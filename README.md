# .NET Tutorial

## Project Structure & the SDK
Modern .NET projects are associated with a project software development kit (SDK). Each project SDK is a set of MSBuild targets and associated tasks that are responsible for compiling, packing, and publishing code.

.NET projects are based on the MSBuild format. Project files, which have extensions like .csproj for C# projects, are in XML format. The root element of an MSBuild project file is the Project element. The Project element has an optional Sdk attribute that specifies which SDK (and version) to use. To use the .NET tools and build your code, set the Sdk attribute to one of the IDs in the [Available SDKs](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#available-sdks) table.


Use NuGet for installing packages

A project can only have one file with top-level statements.

Use dotnet watch to run project with hot reload (similar to npm run dev) and dotnet run to run project with no reload on changes (similar to npm start)

Both file-scoped namespace (single-line statement) and block-scoped namespace (scopes code to its curly braces) can be used within a project, but not within the same file. It is recommended to use file-scoped namespaces from .NET 10 for clean, readable code because it is exceptionally rare to declare more than one namespace per file in modern programming. You should only use block-scoped syntax if you explicitly intend to host multiple unrelated namespaces within the exact same file.

## Concepts & Official Docs

The project exercises the following ASP.NET Core / EF Core concepts. Use these as your primary reference — they are the authoritative sources for everything used here.

- ASP.NET Core fundamentals — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/
- Dependency Injection — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
- Middleware pipeline — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
- Routing & controllers — https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing
- Model binding / validation — https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
- EF Core — https://learn.microsoft.com/en-us/ef/core/
- EF Core migrations — https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- ASP.NET Identity — https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity
- JWT bearer auth — https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication
- Authorization with roles — https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
- In-memory caching — https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory
- Distributed (Redis) caching — https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed
- Rate limiting — https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
- File uploads — https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
- Background tasks / queues (`IHostedService`, `BackgroundService`) — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
- Channels (in-process producer/consumer queues) — https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
- Configuration & options — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
- Environment variables configuration provider — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?#evcp
- Use multiple environments (`ASPNETCORE_ENVIRONMENT`) — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments
- Safe storage of app secrets in development (Secret Manager) — https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets
- OpenAPI / Swagger — https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview

## Configuration layers: `appsettings.json` vs `.env`

ASP.NET Core builds its `IConfiguration` from a chain of providers, with later sources overriding earlier ones. The default order set up by `WebApplication.CreateBuilder` is:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (selected by `ASPNETCORE_ENVIRONMENT`)
3. User Secrets (Development only)
4. Environment variables
5. Command-line arguments

This project splits responsibilities across two of those layers:

| File | Role | Format | Checked in | Read by |
|--|--|--|--|--|
| `appsettings.json` | Framework defaults (`Logging`, `AllowedHosts`, etc.) | Hierarchical JSON | Yes | The framework via `IConfiguration` |
| `appsettings.{Environment}.json` | Per-environment overrides on top of `appsettings.json` | Same as above | Yes | Same as above |
| `.env` | Local app secrets (`JWT_SECRET`, `DB_CONNECTION_STRING`, etc.) | Flat `KEY=VALUE` | No (gitignored) | `AppConfig` via `Environment.GetEnvironmentVariable(...)` |

`.env` is loaded by the `DotNetEnv` package at the top of `Program.cs`, which sets each `KEY=VALUE` as a process environment variable. The ASP.NET-native equivalent in development is the Secret Manager (linked above); in production, you'd typically rely on real environment variables, Azure Key Vault, or another secret store.

**When to put a value where:**

- **`appsettings.json`** — non-sensitive defaults that apply to every environment (logging levels, `AllowedHosts`, default feature flags, internal timeouts, public URLs that rarely change).
- **`appsettings.{Environment}.json`** — non-sensitive values that legitimately differ per environment (verbose logging in `Development`, a staging-only feature flag, environment-specific public endpoints). Keep them under the same JSON keys as `appsettings.json` so they overlay correctly.
- **`.env` / environment variables / Secret Manager** — anything sensitive (`JWT_SECRET`, DB connection strings with credentials, third-party API keys) and anything that varies per machine or per deployment. **Never commit these.** Prefer Secret Manager for local development; use real environment variables (or a secret store) in production.

Rule of thumb: if the value is safe to publish on GitHub, it belongs in an `appsettings.*.json`. Otherwise, it belongs in `.env` / Secret Manager / a secret store.

## Important notes

- **JWT secret length** — `JWT_SECRET` must be at least 32 ASCII characters. HMAC-SHA256 requires a 256-bit key; shorter secrets cause the app to fail fast on startup.
- **Issuer / audience validation is disabled** — this tutorial mints tokens without an issuer or audience, so both `ValidateIssuer` and `ValidateAudience` are set to `false` in `Program.cs`. In production, configure both ends with matching values instead.
- **Role-based auth** — role strings are centralized in `UserRoles` (see `Constants.cs`). Always reference `UserRoles.Admin` / `UserRoles.User` instead of hardcoded strings.
- **Lockout + rate limiting** — login is protected by both ASP.NET Identity lockout (5 attempts → 5-minute lock) and a fixed-window rate limiter (5 requests / minute / IP, returns HTTP 429).
- **Error envelope** — all unhandled exceptions are caught by `ErrorHandlingMiddleware` and returned as `{ message, path, errors, timeStamp }`. Throw `ApiException` (or one of its subclasses in `Exceptions/ApiExceptions.cs`) from service code to set a specific status code and field-level errors.