// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Text;

namespace JDEV.EFMigrationRuntimeSchema
{
    public class EFCommand
    {
        public async Task<EFJsonOutput?> ExecuteAsync(string command)
        {
            if (!command.Contains("--json"))
                command += " --json";

            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"ef {command}", // replace 'tool-name' with your .NET tool name
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Process process = new()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            var outputJson = new StringBuilder();
            var jsonStarted = false;
            process.OutputDataReceived += (sender, args) =>
            {
                Console.WriteLine(args.Data);
                if (args.Data is not null
                        && args.Data.StartsWith('{'))
                    jsonStarted = true;
                if (jsonStarted)
                    outputJson.Append(args.Data);
                if (args.Data is not null
                        && args.Data.StartsWith('}'))
                    jsonStarted = false;

            };

            process.Start();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync();
          
            var jsonString = outputJson.ToString();
            if (jsonString.Length > 0)
            {
                var outputJsonParsed = System.Text.Json.JsonSerializer.Deserialize<EFJsonOutput>(outputJson.ToString());
                return outputJsonParsed;
            }
            return null;
        }
    }
}