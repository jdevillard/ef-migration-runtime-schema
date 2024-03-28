using JDEV.EFMigrationRuntimeSchema;
using System.CommandLine;

var interfaceNameOption = new Option<String?>(
    name: "--interface",
    description: "The name of the interface to inject")
{
    IsRequired = true,
};

var migrationFileNameOption = new Option<String?>(
    name: "--migrationFile",
    description: "The Path to the migration file");

var efOptions = new Option<String?>(
    name: "--efOptions",
    description: "ef command line command and arguments");

var rootCommand = new RootCommand("CLI for ef-migration-runtime-schema");
rootCommand.AddOption(efOptions);
rootCommand.AddOption(interfaceNameOption);
rootCommand.AddOption(migrationFileNameOption);

rootCommand.SetHandler(async (interfaceName, migrationFileName, efOptions) =>
{
    await ExecuteMigration(interfaceName!, migrationFileName, efOptions);
},
           interfaceNameOption,migrationFileNameOption, efOptions);

return await rootCommand.InvokeAsync(args);

static async Task ExecuteMigration(string interfaceName, string? migrationFileName ,string? efOptions)
{
    Console.WriteLine("Starting ef-migration-runtime-schema tools");
    string? migationPath = migrationFileName;

    if (migationPath == null 
        && efOptions!=null)
    {
        Console.WriteLine("Launching EF Tools Command");
        var efResult = await (new EFCommand()).ExecuteAsync(efOptions);
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
