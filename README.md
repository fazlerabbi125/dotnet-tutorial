# .NET Tutorial

## Project Structure & the SDK
Modern .NET projects are associated with a project software development kit (SDK). Each project SDK is a set of MSBuild targets and associated tasks that are responsible for compiling, packing, and publishing code.

.NET projects are based on the MSBuild format. Project files, which have extensions like .csproj for C# projects, are in XML format. The root element of an MSBuild project file is the Project element. The Project element has an optional Sdk attribute that specifies which SDK (and version) to use. To use the .NET tools and build your code, set the Sdk attribute to one of the IDs in the [Available SDKs](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#available-sdks) table.


Use NuGet for installing packages


Overview and Features of EF Core
EF Core: EF Core is an open-source ORM tool in the .NET ecosystem. It enables developers to interact with relational databases using C# objects, simplifying code by eliminating direct SQL queries.

Key Features of EF Core:

LINQ (Language Integrated Query): Integrates query capabilities directly into C# code, making queries type-safe and readable.

Database Migrations: Allows the database schema to evolve alongside application development, supporting changes like table additions or modifications.

Change Tracking: Automatically tracks changes made to objects, streamlining updating the database when data changes.

Advantages of EF Core EF Core offers ease of use, flexibility, and maintainability. It enables database interactions through C# objects, reducing SQL complexity, supporting multiple databases (such as SQL Server, PostgreSQL, and SQLite), and improving scalability.
https://learn.microsoft.com/en-us/ef/core/