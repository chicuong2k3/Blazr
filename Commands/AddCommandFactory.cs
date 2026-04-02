using Blazr.Services;
using Spectre.Console;
using System.CommandLine;
using System.IO;

namespace Blazr.Commands;

internal static class AddCommandFactory
{
    public static Command Create(FeatureCatalog catalog, FeatureInstaller installer)
    {
        var featureArgument = new Argument<string>("feature", "Feature name to install.")
            .FromAmong(catalog.GetFeatureKeys().ToArray());

        var projectOption = new Option<DirectoryInfo?>(
            name: "--project",
            description: "Project directory or .csproj path. Defaults to the current directory.");

        var dryRunOption = new Option<bool>(
            name: "--dry-run",
            description: "Preview the changes without writing files.");

        var command = new Command("add", "Install a supported Blazor capability into a project.");
        command.AddArgument(featureArgument);
        command.AddOption(projectOption);
        command.AddOption(dryRunOption);

        command.SetHandler(async (string feature, DirectoryInfo? project, bool dryRun) =>
        {
            var request = new InstallRequest(feature, project?.FullName, dryRun);
            var result = await installer.InstallAsync(request);

            if (!result.Success)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(result.Message)}[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[green]{Markup.Escape(result.Message)}[/]");

            if (result.Operations.Count > 0)
            {
                var table = new Table().RoundedBorder();
                table.AddColumn("Action");
                table.AddColumn("Path");

                foreach (var operation in result.Operations)
                {
                    table.AddRow(operation.Kind, operation.Path);
                }

                AnsiConsole.Write(table);
            }

            if (result.NextSteps.Count > 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule("Next Steps"));

                foreach (var step in result.NextSteps)
                {
                    AnsiConsole.MarkupLine($"[yellow]-[/] {Markup.Escape(step)}");
                }
            }
        }, featureArgument, projectOption, dryRunOption);

        return command;
    }
}
