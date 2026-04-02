using Blazr.Services;
using Spectre.Console;
using System.CommandLine;
using System.IO;

namespace Blazr.Commands;

internal static class NewCommandFactory
{
    private static readonly string[] SupportedFrameworks = ["net10.0", "net9.0", "net8.0"];
    private static readonly string[] SupportedRenderModes = ["Auto", "Server", "WebAssembly"];
    private static readonly string[] SupportedHttpClients = ["none", "httpclient", "refit", "flurl"];

    public static Command Create(BlueprintTemplateService templateService)
    {
        var nameArgument = new Argument<string?>("name", "Project name.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        var frameworkOption = new Option<string>(
            aliases: ["--framework", "-F"],
            getDefaultValue: () => "net10.0",
            description: "Target framework: net8.0, net9.0, or net10.0.");

        var renderModeOption = new Option<string>(
            aliases: ["--render-mode", "-R"],
            getDefaultValue: () => "Auto",
            description: "Render mode: Server, WebAssembly, or Auto.");

        var includeShowcaseOption = new Option<bool>(
            aliases: ["--include-showcase", "-I"],
            getDefaultValue: () => true,
            description: "Include showcase/demo pages.");

        var httpClientOption = new Option<string>(
            name: "--http-client",
            getDefaultValue: () => "none",
            description: "HTTP client strategy: none, httpclient, refit, or flurl.");

        var outputOption = new Option<DirectoryInfo?>(
            name: "--output",
            description: "Parent directory where the project should be created.");

        var skipInstallOption = new Option<bool>(
            name: "--skip-template-install",
            description: "Skip `dotnet new install BlazorBlueprint.Templates`.");

        var dryRunOption = new Option<bool>(
            name: "--dry-run",
            description: "Preview the template commands without running them.");

        var interactiveOption = new Option<bool>(
            name: "--interactive",
            description: "Run an interactive setup wizard.");

        var withTailwindOption = new Option<bool>(
            name: "--with-tailwind",
            description: "Install the Tailwind v4 standalone setup after project creation.");

        var withBlueprintOption = new Option<bool>(
            name: "--with-blueprint",
            description: "Install Blazor Blueprint packages and app wiring after project creation.");

        var withAuthOption = new Option<bool>(
            name: "--with-auth",
            description: "Install the auth starter after project creation.");

        var withPwaOption = new Option<bool>(
            name: "--with-pwa",
            description: "Install Bit.Bswup PWA update progress setup after project creation.");

        var withBesqlOption = new Option<bool>(
            name: "--with-besql",
            description: "Install Bit.Besql browser SQLite starter after project creation.");

        var withTestOption = new Option<bool>(
            name: "--with-test",
            description: "Create a sibling xUnit test project after project creation.");

        var command = new Command("new", "Create a new Blazor Blueprint project.");
        command.AddArgument(nameArgument);
        command.AddOption(frameworkOption);
        command.AddOption(renderModeOption);
        command.AddOption(includeShowcaseOption);
        command.AddOption(httpClientOption);
        command.AddOption(outputOption);
        command.AddOption(skipInstallOption);
        command.AddOption(dryRunOption);
        command.AddOption(interactiveOption);
        command.AddOption(withBlueprintOption);
        command.AddOption(withTailwindOption);
        command.AddOption(withAuthOption);
        command.AddOption(withPwaOption);
        command.AddOption(withBesqlOption);
        command.AddOption(withTestOption);

        command.SetHandler(async context =>
        {
            var name = context.ParseResult.GetValueForArgument(nameArgument);
            var framework = context.ParseResult.GetValueForOption(frameworkOption) ?? "net10.0";
            var renderMode = context.ParseResult.GetValueForOption(renderModeOption) ?? "Auto";
            var includeShowcase = context.ParseResult.GetValueForOption(includeShowcaseOption);
            var httpClient = context.ParseResult.GetValueForOption(httpClientOption) ?? "none";
            var output = context.ParseResult.GetValueForOption(outputOption);
            var skipTemplateInstall = context.ParseResult.GetValueForOption(skipInstallOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var interactive = context.ParseResult.GetValueForOption(interactiveOption);
            var withBlueprint = context.ParseResult.GetValueForOption(withBlueprintOption);
            var withTailwind = context.ParseResult.GetValueForOption(withTailwindOption);
            var withAuth = context.ParseResult.GetValueForOption(withAuthOption);
            var withPwa = context.ParseResult.GetValueForOption(withPwaOption);
            var withBesql = context.ParseResult.GetValueForOption(withBesqlOption);
            var withTest = context.ParseResult.GetValueForOption(withTestOption);

            if (!SupportedHttpClients.Contains(httpClient, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[red]Invalid http client. Use none, httpclient, refit, or flurl.[/]");
                return;
            }

            NewProjectRequest config;
            try
            {
                var useWizard = interactive || string.IsNullOrWhiteSpace(name);

                config = useWizard
                    ? PromptForProjectConfig(
                        name,
                        framework,
                        renderMode,
                        includeShowcase,
                        httpClient,
                        output?.FullName,
                        skipTemplateInstall,
                        dryRun,
                        withBlueprint,
                        withTailwind,
                        withAuth,
                        withPwa,
                        withBesql,
                        withTest)
                    : BuildRequest(
                        name!,
                        framework,
                        renderMode,
                        includeShowcase,
                        httpClient,
                        output?.FullName,
                        skipTemplateInstall,
                        dryRun,
                        withBlueprint,
                        withTailwind,
                        withAuth,
                        withPwa,
                        withBesql,
                        withTest);
            }
            catch (OperationCanceledException exception)
            {
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(exception.Message)}[/]");
                return;
            }

            var result = await templateService.CreateAsync(config);

            if (!result.Success)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(result.Message)}[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[green]{Markup.Escape(result.Message)}[/]");

            if (result.Commands.Count > 0)
            {
                var table = new Table().RoundedBorder();
                table.AddColumn("Command");

                foreach (var item in result.Commands)
                {
                    table.AddRow(Markup.Escape(item));
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
        });

        return command;
    }

    private static NewProjectRequest PromptForProjectConfig(
        string? initialName,
        string initialFramework,
        string initialRenderMode,
        bool includeShowcase,
        string initialHttpClient,
        string? outputDirectory,
        bool skipTemplateInstall,
        bool dryRun,
        bool withBlueprint,
        bool withTailwind,
        bool withAuth,
        bool withPwa,
        bool withBesql,
        bool withTest)
    {
        AnsiConsole.Write(
            new Panel("[bold]Blazr[/] project wizard\nCreate a Blazor app with Blueprint styling and optional integrations.")
                .Border(BoxBorder.Rounded)
                .Header("Create Project"));

        var name = string.IsNullOrWhiteSpace(initialName)
            ? AnsiConsole.Prompt(
                new TextPrompt<string>("Project name:")
                    .PromptStyle("green")
                    .Validate(static value =>
                        string.IsNullOrWhiteSpace(value)
                            ? ValidationResult.Error("[red]Project name is required.[/]")
                            : ValidationResult.Success()))
            : initialName;

        var selectedFramework = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Framework")
                .AddChoices(OrderChoices(SupportedFrameworks, initialFramework))
                .HighlightStyle("green"));

        var selectedRenderMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Render mode")
                .AddChoices(OrderChoices(SupportedRenderModes, initialRenderMode))
                .HighlightStyle("green"));

        var selectedShowcase = AnsiConsole.Confirm("Include showcase pages?", includeShowcase);

        var selectedHttpClient = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("HTTP client")
                .AddChoices(OrderChoices(SupportedHttpClients, initialHttpClient))
                .UseConverter(static value => value switch
                {
                    "none" => "None",
                    "httpclient" => "Built-in HttpClient",
                    "refit" => "Refit",
                    "flurl" => "Flurl.Http",
                    _ => value
                })
                .HighlightStyle("green"));

        var extrasPrompt = new MultiSelectionPrompt<string>()
            .Title("Select extras")
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to continue)[/]")
            .NotRequired()
            .AddChoices("blueprint", "tailwind", "auth", "pwa", "besql", "test")
            .UseConverter(static feature => feature switch
            {
                "blueprint" => "Blazor Blueprint components",
                "tailwind" => "Tailwind CSS setup",
                "auth" => "Auth starter",
                "pwa" => "Bit.Bswup PWA update progress",
                "besql" => "Bit.Besql browser SQLite",
                "test" => "xUnit test project",
                _ => feature
            });

        if (withBlueprint)
        {
            extrasPrompt.Select("blueprint");
        }

        if (withTailwind)
        {
            extrasPrompt.Select("tailwind");
        }

        if (withAuth)
        {
            extrasPrompt.Select("auth");
        }

        if (withPwa)
        {
            extrasPrompt.Select("pwa");
        }

        if (withBesql)
        {
            extrasPrompt.Select("besql");
        }

        if (withTest)
        {
            extrasPrompt.Select("test");
        }

        var selectedFeatures = AnsiConsole.Prompt(extrasPrompt).ToList();

        if (!string.Equals(selectedHttpClient, "none", StringComparison.OrdinalIgnoreCase))
        {
            selectedFeatures.Add(selectedHttpClient.ToLowerInvariant());
        }

        var selectedOutput = AnsiConsole.Confirm("Create project in current directory?", string.IsNullOrWhiteSpace(outputDirectory))
            ? null
            : AnsiConsole.Prompt(
                new TextPrompt<string>("Output directory:")
                    .PromptStyle("green"));

        var selectedSkipInstall = AnsiConsole.Confirm("Skip template install step?", skipTemplateInstall);

        ShowSummary(
            name,
            selectedFramework,
            selectedRenderMode,
            selectedShowcase,
            selectedHttpClient,
            string.IsNullOrWhiteSpace(selectedOutput) ? Directory.GetCurrentDirectory() : selectedOutput,
            selectedSkipInstall,
            selectedFeatures);

        if (!AnsiConsole.Confirm("Proceed?", true))
        {
            throw new OperationCanceledException("Project creation cancelled.");
        }

        return new NewProjectRequest(
            name,
            selectedFramework,
            selectedRenderMode,
            selectedShowcase,
            selectedOutput,
            selectedSkipInstall,
            dryRun,
            selectedHttpClient,
            selectedFeatures);
    }

    private static NewProjectRequest BuildRequest(
        string name,
        string framework,
        string renderMode,
        bool includeShowcase,
        string httpClient,
        string? outputDirectory,
        bool skipTemplateInstall,
        bool dryRun,
        bool withBlueprint,
        bool withTailwind,
        bool withAuth,
        bool withPwa,
        bool withBesql,
        bool withTest)
    {
        var selectedFeatures = BuildSelectedFeatures(withBlueprint, withTailwind, withAuth, withPwa, withBesql, withTest).ToList();

        if (!string.Equals(httpClient, "none", StringComparison.OrdinalIgnoreCase))
        {
            selectedFeatures.Add(httpClient.ToLowerInvariant());
        }

        return new NewProjectRequest(
            name,
            framework,
            renderMode,
            includeShowcase,
            outputDirectory,
            skipTemplateInstall,
            dryRun,
            httpClient,
            selectedFeatures);
    }

    private static IReadOnlyList<string> BuildSelectedFeatures(bool withBlueprint, bool withTailwind, bool withAuth, bool withPwa, bool withBesql, bool withTest)
    {
        var features = new List<string>();

        if (withBlueprint)
        {
            features.Add("blueprint");
        }

        if (withTailwind)
        {
            features.Add("tailwind");
        }

        if (withAuth)
        {
            features.Add("auth");
        }

        if (withPwa)
        {
            features.Add("pwa");
        }

        if (withBesql)
        {
            features.Add("besql");
        }

        if (withTest)
        {
            features.Add("test");
        }

        return features;
    }

    private static void ShowSummary(
        string name,
        string framework,
        string renderMode,
        bool includeShowcase,
        string httpClient,
        string outputDirectory,
        bool skipTemplateInstall,
        IReadOnlyCollection<string> selectedFeatures)
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Project", name);
        table.AddRow("Framework", framework);
        table.AddRow("Render Mode", renderMode);
        table.AddRow("Showcase", includeShowcase ? "Yes" : "No");
        table.AddRow("HTTP Client", FormatHttpClient(httpClient));
        table.AddRow("Output", outputDirectory);
        table.AddRow("Template Install", skipTemplateInstall ? "Skip" : "Install");
        table.AddRow("Extras", selectedFeatures.Count == 0 ? "None" : string.Join(", ", selectedFeatures));

        AnsiConsole.Write(new Rule("Summary"));
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static string FormatHttpClient(string value) => value.ToLowerInvariant() switch
    {
        "none" => "None",
        "httpclient" => "Built-in HttpClient",
        "refit" => "Refit",
        "flurl" => "Flurl.Http",
        _ => value
    };

    private static IReadOnlyList<string> OrderChoices(IReadOnlyList<string> values, string preferred)
    {
        if (string.IsNullOrWhiteSpace(preferred) || !values.Contains(preferred, StringComparer.Ordinal))
        {
            return values;
        }

        return values
            .OrderByDescending(value => string.Equals(value, preferred, StringComparison.Ordinal))
            .ToArray();
    }
}
