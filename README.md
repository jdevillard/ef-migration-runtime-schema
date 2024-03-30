[![NuGet Version](https://img.shields.io/nuget/v/ef-migration-runtime-schema)](https://www.nuget.org/packages/ef-migration-runtime-schema)

# Overview

`ef-migration-runtime-schema` is a CLI tool that wraps Entity Framework Core command-line tool in order to support changing the schema of the EF Core Migrations at runtime.

The Entity Framework Core command-line tool creates [migration files](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#add-a-migration) that contain classes where the `up` and `down` methods are designed to reflect incremental changes to your model:

```c#
migrationBuilder.AddColumn<string>(
    name: "FullName",
    schema: "mySchemaName",
    table: "Customers",
    nullable: true);

migrationBuilder.DropColumn(
    schema: "mySchemaName",
    name: "Name",
    table: "Customers");
```

Unfortunately, the database schema name, _e.g_ `mySchemaName`, is hard coded as a string constant throughout the migration files.

The motivation for this project comes from a very good blog post that oulines a strategy to automate the changes required to the EF Core Migrations classes, in order to support specifying the schema at runtime.

> https://medium.com/@pawel.gerr/entity-framework-core-changing-db-migration-schema-at-runtime-50cd28cd18bf

## Changing the schema at runtime

In order to support specifying a database schema at runtime to the EF Core Migrations, the `ef-migration-runtime-schema` CLI tool performs the following changes to the generated code:

- Adds a constructor to the migrations classes in order to inject an instance of a custom interface used to retrieve the schema name.
- Adds a new class that inherits from `MigrationsAssembly` in order to support the new constructor.

When running this tool, you are specifying a reference to a fully qualified interface that MUST exists at runtime.

You are responsible for supplying the definition for that custom interface like so:

```c#
namespace EfCoreRuntimeSchema
{
  internal interface IDbContextSchema 
  {
    /// <summary>
    /// Name of the Schema
    /// </summary>
    public string Schema { get; }
  }
}
```

The tool can be invoked in one of two ways:

- Using the path to the generated migrations file

```bash
dotnet ef migrations add AddUserName -o <path-to-migrations-file>

dotnet ef-core-runtime-schema \
  --interface EfCoreRuntimeSchema.IDbContextSchema \
  --migrations-file <path-to-migrations-file>
```

- Passing the EF Core migrations arguments after the `--` token, when generating a new migration

```bash
dotnet ef-core-runtime-schema \
  --interface EfCoreRuntimeSchema.IDbContextSchema \
  -- migrations add AddUserName -o <path-to-migrations-file>"
```

The CLIT tool will make the following changes to the migrations file:

```patch
public partial class AddUserName : Migration
{
+   private readonly FullNamespace.Interface _schema;
+
+   /// <inheritdoc />
+   public AddUserName(FullNamespace.Interface schema)
+   {
+      _schema = schema;
+   }

  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
    name: "FullName",
-   schema: "mySchemaName",
+   schema: _schema.Schema,
    table: "Customers",
    nullable: true);
  }
}
```

## Usage

The CLI help screen is shown hereafter:

```
Description:
  CLI for ef-migration-runtime-schema

Usage:
  ef-migration-runtime-schema [options] [-- <ef-options>...]
  <!-- dotnet ef-core-runtime-schema [options] [-- <ef-options>...] -->

Arguments:
  <ef-options>  EF Core CLI command and arguments

Options:
  --interface <interface> (REQUIRED)   The fully qualified name of the interface to inject
  --migrations-file <migrations-file>  The path to the migrations file
  --version                            Show version information
  -?, -h, --help                       Show help and usage information
  ```