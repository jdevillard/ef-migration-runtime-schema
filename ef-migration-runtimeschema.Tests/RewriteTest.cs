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


        [Fact]
        public void ShouldReplaceSchemaInForeignKeyTableConstraint()
        {
            var classString = @"
public partial class AddUserName : Migration
{
    
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    migrationBuilder.CreateTable(
        name: ""OrganizationUser"",
        schema: _schema.Schema,
        columns: table => new
        {
            OrganizationsId = table.Column<string>(type: ""text"", nullable: false),
            UsersId = table.Column<string>(type: ""text"", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey(""PK_OrganizationUser"", x => new { x.OrganizationsId, x.UsersId });
            table.ForeignKey(
                name: ""FK_OrganizationUser_Organization_OrganizationsId"",
                column: x => x.OrganizationsId,
                principalSchema: ""defaultSchema"",
                principalTable: ""Organization"",
                principalColumn: ""Id"",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: ""FK_OrganizationUser_Users_UsersId"",
                column: x => x.UsersId,
                principalSchema: ""defaultSchema"",
                principalTable: ""Users"",
                principalColumn: ""Id"",
                onDelete: ReferentialAction.Cascade);
        });
}";

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
    migrationBuilder.CreateTable(
        name: ""OrganizationUser"",
        schema: _schema.Schema,
        columns: table => new
        {
            OrganizationsId = table.Column<string>(type: ""text"", nullable: false),
            UsersId = table.Column<string>(type: ""text"", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey(""PK_OrganizationUser"", x => new { x.OrganizationsId, x.UsersId });
            table.ForeignKey(
                name: ""FK_OrganizationUser_Organization_OrganizationsId"",
                column: x => x.OrganizationsId,
                principalSchema: _schema.Schema,
                principalTable: ""Organization"",
                principalColumn: ""Id"",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: ""FK_OrganizationUser_Users_UsersId"",
                column: x => x.UsersId,
                principalSchema: _schema.Schema,
                principalTable: ""Users"",
                principalColumn: ""Id"",
                onDelete: ReferentialAction.Cascade);
        });
}";

            var result = MigrationCommand.RewriteSyntaxtNode(interfaceName, "testPath", classString);
            var str = result.ToFullString();

            Assert.Equal(final, str, ignoreLineEndingDifferences: true);
        }


    }
}