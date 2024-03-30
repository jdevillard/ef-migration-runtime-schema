using JDEV.EFMigrationRuntimeSchema;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;

var interfaceNameOption = new Option<String?>(
    name: "--interface",
    description: "The fully qualified name of the interface to inject")
{
    IsRequired = true,
};

var migrationsFileNameOption = new Option<String?>(
    name: "--migrations-file",
    description: "The path to the migrations file");

var efArguments = new Argument<String[]?>(
    name: "ef-options",
    description: "EF Core CLI command and arguments");

var rootCommand = new RootCommand("CLI for ef-migration-runtime-schema");
rootCommand.AddOption(interfaceNameOption);
rootCommand.AddOption(migrationsFileNameOption);
rootCommand.AddArgument(efArguments);

rootCommand.SetHandler(async (interfaceName, migrationFileName, efOptions) =>
{
    await ExecuteMigration(interfaceName!, migrationFileName, efOptions);
},
           interfaceNameOption, migrationsFileNameOption, efArguments);

var parser = new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseHelp(ctx =>
        {
            ctx.HelpBuilder.CustomizeLayout(
                _ =>
                [
                    HelpBuilder.Default.SynopsisSection(),
                    // replacing the HelpBuilder.Default.CommandUsageSection() delegate
                    _ => {
                        _.Output.WriteLine("Usage:");
                        _.Output.WriteLine("  ef-migration-runtime-schema [options] [-- <ef-options>...]");
                        _.Output.WriteLine("  dotnet ef-core-runtime-schema [options] [-- <ef-options>...]");
                    },
                    HelpBuilder.Default.CommandArgumentsSection(),
                    HelpBuilder.Default.OptionsSection(),
                ]);
        })
        .Build();

return await parser.InvokeAsync(args);

static async Task ExecuteMigration(string interfaceName, string? migrationFileName ,string[]? efOptions)
{
    Console.WriteLine("Starting ef-migration-runtime-schema tools");
    string? migationPath = migrationFileName;

    if (migationPath == null 
        && efOptions?.Length > 0)
    {
        Console.WriteLine("Launching EF Tools Command");
        var arguments = String.Join(" ", efOptions!);
        var efResult = await new EFCommand().ExecuteAsync(arguments);
        migationPath = efResult?.MigrationFile;
    }

    if(migationPath == null)
    {
        Console.WriteLine("No Migration File Found");
        Environment.Exit(0);
    }

    var command = new MigrationCommand();
    command.Execute(interfaceName, migationPath);
    Console.WriteLine("Done");
}
