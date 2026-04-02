using Blazr.Services;
using Spectre.Console;
using System.CommandLine;
using System.IO;

namespace Blazr.Commands;

internal static class ListCommandFactory
{
    public static Command Create(
        FeatureCatalog catalog,
        ProjectLocator locator,
        ManifestService manifestService)
    {
        var projectOption = new Option<DirectoryInfo?>(
            name: "--project",
            description: "Project directory or .csproj path used to show installed features.");

        var command = new Command("list", "List supported features and show installation status.");
        command.AddOption(projectOption);

        command.SetHandler((DirectoryInfo? project) =>
        {
            var resolution = TryResolveProject(project?.FullName, locator);
            var manifest = resolution is null
                ? null
                : manifestService.Load(resolution.ProjectDirectory);
            var installed = manifest?.InstalledFeatures.ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (resolution is not null)
            {
                var label = resolution.ProjectType switch
                {
                    BlazorProjectType.WebApp => "Blazor Web App",
                    BlazorProjectType.Server => "Blazor Server",
                    BlazorProjectType.Wasm => "Blazor WebAssembly",
                    _ => "Unknown"
                };

                AnsiConsole.MarkupLine($"[blue]Detected project type:[/] {label}");
                AnsiConsole.WriteLine();
            }

            var table = new Table().RoundedBorder();
            table.AddColumn("Feature");
            table.AddColumn("Status");
            table.AddColumn("Description");

            foreach (var feature in catalog.GetAll())
            {
                var status = installed.Contains(feature.Key) ? "[green]installed[/]" : "[grey]available[/]";
                table.AddRow(feature.Key, status, feature.Description);
            }

            AnsiConsole.Write(table);
        }, projectOption);

        return command;
    }

    private static ProjectResolution? TryResolveProject(
        string? projectPath,
        ProjectLocator locator)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            projectPath = Directory.GetCurrentDirectory();
        }

        var resolution = locator.Resolve(projectPath);
        if (!resolution.Success)
        {
            return null;
        }

        return resolution;
    }
}
