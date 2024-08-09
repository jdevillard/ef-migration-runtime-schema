using JDEV.EFMigrationRuntimeSchema;

namespace ef_migration_runtimeschema.Tests
{
    public class RewriteTest
    {

        const string completeClassString = @"
public partial class AddUserName : Migration
   

/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
    name: ""FullName"",
    schema:""schema"",
    table: ""Customers"",
    nullable: true);
}
}
";
        const string interfaceName = "InterfaceName";

        [Fact]
        public void ShouldAddConstructor()
        {
            var constructorClassString = @"
public partial class AddUserName : Migration
{
}
";

            var final = @"
public partial class AddUserName : Migration
{
    private readonly InterfaceName _schema;

    /// <inheritdoc />
    public AddUserName(InterfaceName schema)
    {
        _schema = schema;
    }
}
";

            var result = MigrationCommand.RewriteSyntaxtNode(interfaceName,"testPath", constructorClassString);
            var str = result.ToFullString();

            Assert.Equal(final, str,ignoreLineEndingDifferences:true);
        }

        [Fact]
        public void ShouldNotAddTwoConstructors()
        {
            var constructorClassString = @"
public partial class AddUserName : Migration
{
private readonly InterfaceName _schema;

/// <inheritdoc />
public AddUserName(InterfaceName schema)
{
    _schema = schema;
}
}
";

            var final = @"
public partial class AddUserName : Migration
{
private readonly InterfaceName _schema;

/// <inheritdoc />
public AddUserName(InterfaceName schema)
{
    _schema = schema;
}
}
";

            var result = MigrationCommand.RewriteSyntaxtNode(interfaceName, "testPath", constructorClassString);
            var str = result.ToFullString();

            Assert.Equal(final, str, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences:true);
        }


        [Fact]
        public void ShouldReplaceSchemaNameInAddColumn()
        {
            var classString = @"
public partial class AddUserName : Migration
{   

/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
        name: ""FullName"",
        schema:""schema"",
        table: ""Customers"",
        nullable: true);
    }
}
";

            var final = @"
public partial class AddUserName : Migration
{
    private readonly InterfaceName _schema;

    /// <inheritdoc />
    public AddUserName(InterfaceName schema)
    {
        _schema = schema;
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
        name: ""FullName"",
        schema:_schema.Schema,
        table: ""Customers"",
        nullable: true);
    }
}
";

            var result = MigrationCommand.RewriteSyntaxtNode(interfaceName, "testPath", classString);
            var str = result.ToFullString();

            Assert.Equal(final, str, ignoreLineEndingDifferences: true);
        }

   

    [Fact]
    public void ShouldReplaceSchemaInMethodEnsureSchema()
    {
        var classString = @"
public partial class AddUserName : Migration
{   

/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
        name: ""schema"");
    }
}
";

        var final = @"
public partial class AddUserName : Migration
{
    private readonly InterfaceName _schema;

    /// <inheritdoc />
    public AddUserName(InterfaceName schema)
    {
        _schema = schema;
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
        name: _schema.Schema);
    }
}
";

        var result = MigrationCommand.RewriteSyntaxtNode(interfaceName, "testPath", classString);
        var str = result.ToFullString();

        Assert.Equal(final, str, ignoreLineEndingDifferences: true);
    }

    }
}