# .NET Tutorial

## Project Structure & the SDK
Modern .NET projects are associated with a project software development kit (SDK). Each project SDK is a set of MSBuild targets and associated tasks that are responsible for compiling, packing, and publishing code.

.NET projects are based on the MSBuild format. Project files, which have extensions like .csproj for C# projects, are in XML format. The root element of an MSBuild project file is the Project element. The Project element has an optional Sdk attribute that specifies which SDK (and version) to use. To use the .NET tools and build your code, set the Sdk attribute to one of the IDs in the [Available SDKs](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#available-sdks) table.


Use NuGet for installing packages