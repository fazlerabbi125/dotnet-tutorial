# LogiTrack Architecture Summary

This project implements a robust, maintainable backend architecture in .NET 10, combining Domain-Driven Design (DDD) principles with a feature-slice folder structure.

## 1. System Layers & Organization
We transitioned from a standard MVC layout to a feature-sliced architecture:
- **`Models/`**: Contains only the core domain entities (`InventoryItem`, `Order`, `ApplicationUser`) and the `TimeStampMixin`. No Data Transfer Objects (DTOs) exist here.
- **`dtos/`**: Dedicated models mapping the exact shape of incoming (Create) and outgoing (Response/Summary) data, preventing over-posting and circular JSON references.
- **`Repositories/`**: Abstracts EF Core logic. We use a generic `IRepository<T>` interface offering immense flexibility—the service layer can pass optional `Expression<Func<T, bool>>` filters or `Include` delegates without leaking `IQueryable` (which violates the repository pattern).
- **`Services/`**: Grouped by feature (`Inventory`, `Orders`, `Auth`). Each feature slice contains its specific Interface, Service implementation, and Controller. The Service handles all business logic, caching, and DTO mapping, keeping the Controller extremely thin.

## 2. Database & EF Core Strategy
- **SQLite**: Configured via the `DB_CONNECTION_STRING` environment variable for easy local development.
- **Fluent API**: We explicitly configure primary keys (`entity.HasKey`) and validation rules (`HasMaxLength`, `IsRequired`) within `AppDbContext.OnModelCreating`.
- **Identity Upgrade**: `AppDbContext` extends `IdentityDbContext<ApplicationUser>`, bringing in Microsoft's proven Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) while maintaining our domain tables.

## 3. Security & Authorization
- **JWT Authentication**: We use `JwtBearer` tokens rather than cookies to support modern stateless APIs (ideal for SPAs or mobile clients).
- **Identity Core**: By registering `.AddIdentityCore<ApplicationUser>()` instead of `.AddIdentity()`, we avoid the framework silently injecting Cookie authentication as the default scheme.
- **Role-Based Access Control (RBAC)**: Mutating endpoints are guarded by `[Authorize]`. Deletion specifically requires `[Authorize(Roles = "Manager")]`.

## 4. Performance Optimizations
- **Caching**: The `InventoryService` utilizes `IMemoryCache` to store the inventory list for 30 seconds. Write operations (Create/Delete) aggressively invalidate this cache to ensure data freshness.
- **Projections**: The Order list endpoint returns an `OrderSummaryDto` rather than eager-loading all items. Full item detail is reserved for the `GetById` endpoint (`OrderDetailDto`).
- **NoTracking Queries**: Read-only repository calls utilize EF Core's `.AsNoTracking()` to drastically reduce memory usage and query execution time.

## 5. Migrations
The project was built over two distinct migrations to demonstrate an evolutionary database approach:
1. `InitialCreate`: Bootstrapped the core domain (`Orders`, `InventoryItems`).
2. `AddIdentity`: Upgraded the context and introduced Identity tables.

## Next Steps for Beginners
Explore the inline documentation throughout the codebase! Files like `Program.cs`, the Controllers, and `AppDbContext.cs` contain Microsoft Learn links and explanations for concepts like Dependency Injection (DI), Routing, and EF Core configurations.
