# Project Architecture Summary

This project is a .NET 10 Web API organized around domain models, DTOs, repositories, services, controllers, and startup seeding. `Program.cs` configures EF Core with SQLite, ASP.NET Core Identity, JWT bearer authentication, in-memory caching, repositories, services, and the interactive `create-admin` command.

## 1. System Layers & Organization
We transitioned from a standard MVC layout to a feature-sliced architecture:
- **`Models/`**: Contains only the core domain entities (`InventoryItem`, `Order`, `ApplicationUser`) and the `TimeStampMixin`. No Data Transfer Objects (DTOs) exist here.
- **`dtos/`**: Dedicated models mapping the exact shape of incoming (Create/Update) and outgoing (Response/Summary) data, preventing over-posting and circular JSON references.
- **`Constants/`**: Application constants live in `Constants.cs`. Cache keys and cache TTL profiles are represented with enums and mapped through helper methods. Inventory and order caches are invalidated after create, update, and delete operations so list/detail reads do not serve stale data.

- **`Commands/`**: Contains command handlers such as `CreateAdminUserCommand` for user management operations with validation and error handling.
- **`Repositories/`**: Abstracts EF Core logic. We use a generic `IRepository<T>` interface offering immense flexibility—the service layer can pass optional `Expression<Func<T, bool>>` filters or `Include` delegates without leaking `IQueryable` (which violates the repository pattern).
- **`Services/`**: Grouped by feature (`Inventory`, `Orders`, `Auth`). Each feature slice contains its specific Interface, Service implementation, and Controller. The Service handles all business logic, caching, and DTO mapping, keeping the Controller extremely thin.

## 2. Database & EF Core Strategy
- **SQLite**: Configured via the `DB_CONNECTION_STRING` environment variable for easy local development.
- **Fluent API**: We explicitly configure primary keys (`entity.HasKey`) and validation rules (`HasMaxLength`, `IsRequired`) within `AppDbContext.OnModelCreating`.
- **Identity Upgrade**: `AppDbContext` extends `IdentityDbContext<ApplicationUser>`, bringing in Microsoft's proven Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) while maintaining our domain tables.
- **Timestamp Management**: The `TimeStampMixin` is automatically managed via `SaveChanges` overrides to set `CreatedAt` on insert and `UpdatedAt` on every modification.

## 3. Security & Authorization
- **JWT Authentication**: We use `JwtBearer` tokens rather than cookies to support modern stateless APIs (ideal for SPAs or mobile clients).
- **Identity Core**: By registering `.AddIdentityCore<ApplicationUser>()` instead of `.AddIdentity()`, we avoid the framework silently injecting Cookie authentication as the default scheme.
- **Role-Based Access Control (RBAC)**: Mutating endpoints are guarded by `[Authorize]`. Deletion specifically requires `[Authorize(Roles = "Manager")]`.
- **Safe JWT Claims**: JWT claims now use null-coalescing operators to prevent null reference exceptions when building tokens.
- **Admin User Management**: Authentication uses Identity users and role-based authorization with centralized `AppRoles` constants. The `dotnet run -- create-admin` command prompts for admin email/password, validates input, ensures the Manager role exists, and creates the admin user without hardcoded credentials. Optional `ADMIN_EMAIL` and `ADMIN_PASSWORD` environment variables are still supported for first-run automatic seeding.

## 4. Performance Optimizations
- **Caching with Constants**: The `InventoryService` utilizes `IMemoryCache` with cache keys defined in `CacheConstants.CacheKeyType` enum and TTL values in `CacheTtl` class. This prevents magic strings and makes cache configuration type-safe.
- **Efficient User Queries**: Fixed inefficient user count check using `CountAsync()` instead of `.Count() == 1` to avoid full table scans.
- **Projections**: The Order list endpoint returns an `OrderSummaryDto` rather than eager-loading all items. Full item detail is reserved for the `GetById` endpoint (`OrderDetailDto`).
- **NoTracking Queries**: Read-only repository calls utilize EF Core's `.AsNoTracking()` to drastically reduce memory usage and query execution time.

## 5. REST API Completeness
- **CRUD Operations**: All resources now support full CRUD operations:
  - **GET** endpoints for retrieving single and multiple resources
  - **POST** endpoints for creating resources with validation
  - **PUT** endpoints for updating existing resources (Manager role required)
  - **DELETE** endpoints for removing resources (Manager role required)
- **Validation**: All DTOs include validation attributes, and the `CreateOrderDto` now requires at least one item.
- **Update DTOs**: `UpdateInventoryItemDto` and `UpdateOrderDto` use nullable properties to support partial updates.

## 6. Migrations
The project was built over two distinct migrations to demonstrate an evolutionary database approach:
1. `InitialCreate`: Bootstrapped the core domain (`Orders`, `InventoryItems`).
2. `AddIdentity`: Upgraded the context and introduced Identity tables.

## 7. Recent Improvements (Code Review) 

### Critical Fixes Applied:
- **JWT Claims Security**: Fixed unsafe null references when building JWT claims by using null-coalescing operators
- **Admin Credential Management**: Moved hardcoded admin credentials to environment variables (`ADMIN_EMAIL`, `ADMIN_PASSWORD`)
- **Authentication Robustness**: Improved user registration with efficient async database queries

### Code Quality Enhancements:
- **Centralized Constants**: Created `CacheConstants.cs` with:
  - `CacheKeyType` enum for type-safe cache key definitions
  - `CacheTtl` class with TimeSpan constants for cache expiration
- **Admin User Creation**: Implemented `CreateAdminUserCommand` for creating admin users interactively with validation:
  - Email format validation
  - Password strength requirements (minimum 6 characters)
  - Confirmation validation
  - Error handling with logging
- **Exception Handling**: Enhanced `DbSeeder` with try-catch blocks and logging for:
  - Database migrations
  - Inventory item seeding
  - Role creation
  - Admin user creation
- **API Endpoints**: Added PUT endpoints for updating both Inventory and Order resources with Manager role authorization
- **DTO Validation**: Enhanced `CreateOrderDto` to require at least one item and added descriptive error messages

### Code Organization:
- New `Constants/` folder for application-wide constants
- New `Commands/` folder for command handlers and business operations
- Improved separation of concerns with update DTOs and service methods

## Next Steps for Beginners
Explore the inline documentation throughout the codebase! Files like `Program.cs`, the Controllers, and `AppDbContext.cs` contain Microsoft Learn links and explanations for concepts like Dependency Injection (DI), Routing, and EF Core configurations.

### Running Admin User Creation
To create an admin user interactively:
1. Ensure the application is running or start it
2. Use the `CreateAdminUserCommand` class with dependency injection
3. Or set environment variables: `ADMIN_EMAIL` and `ADMIN_PASSWORD` before starting the app for automatic seeding

Example environment variables:
```bash
set ADMIN_EMAIL=admin@example.com
set ADMIN_PASSWORD=YourSecurePassword@123
```