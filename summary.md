# LogiTrack Architecture Summary

LogiTrack is an ASP.NET Core Web API organized around domain models, DTOs, repositories, services, controllers, and startup seeding. `Program.cs` configures EF Core with SQLite, ASP.NET Core Identity, JWT bearer authentication without issuer/audience requirements, in-memory caching, repositories, services, and the interactive `create-admin` command.

The data layer uses `AppDbContext` with EF Core migrations for inventory, orders, timestamps, and Identity tables. Repositories wrap EF Core access, while services contain business logic, DTO mapping, caching, cache invalidation, and calls to `InventoryItem.DisplayInfo()` when item details need to be printed during seeding or mutations.

Authentication uses Identity users and role-based authorization with centralized `AppRoles` constants. The `dotnet run -- create-admin` command prompts for admin email/password, validates input, ensures the Manager role exists, and creates the admin user without hardcoded credentials. Optional `ADMIN_EMAIL` and `ADMIN_PASSWORD` environment variables are still supported for first-run automatic seeding.

Application constants live in `Constants.cs` and `Constants/CacheConstants.cs`. Cache keys and cache TTL profiles are represented with enums and mapped through helper methods. Inventory and order caches are invalidated after create, update, and delete operations so list/detail reads do not serve stale data.
