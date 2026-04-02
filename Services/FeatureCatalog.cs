using System.Text;

namespace Blazr.Services;

internal sealed class FeatureCatalog
{
    private readonly CommandExecutionService commandExecutionService;

    public IReadOnlyCollection<FeatureDefinition> GetAll() => _features.Values;

    public IEnumerable<string> GetFeatureKeys() => _features.Keys.OrderBy(static key => key);

    public bool TryGet(string key, out FeatureDefinition feature) =>
        _features.TryGetValue(key, out feature!);

    private readonly Dictionary<string, FeatureDefinition> _features = new(StringComparer.OrdinalIgnoreCase);

    public FeatureCatalog(CommandExecutionService commandExecutionService)
    {
        this.commandExecutionService = commandExecutionService;

        _features["tailwind"] = new(
            "tailwind",
            "Adds Tailwind v4 standalone CLI setup for Blazor Blueprint-style utility usage.",
            (context, dryRun, step, totalSteps) => InstallTailwindAsync(context, dryRun, step, totalSteps));
        _features["blueprint"] = new(
            "blueprint",
            "Adds Blazor Blueprint components, icons, services, imports, styles, and portal host wiring.",
            (context, dryRun, step, totalSteps) => InstallBlueprintAsync(context, dryRun, step, totalSteps));
        _features["auth"] = new(
            "auth",
            "Adds a starter authentication state provider and setup notes.",
            (context, dryRun, step, totalSteps) => InstallAuthAsync(context, dryRun, step, totalSteps));
        _features["pwa"] = new(
            "pwa",
            "Adds Bit.Bswup service-worker update progress setup for Blazor PWAs.",
            (context, dryRun, step, totalSteps) => InstallPwaAsync(context, dryRun, step, totalSteps));
        _features["besql"] = new(
            "besql",
            "Adds Bit.Besql starter setup for EF Core + SQLite in the browser.",
            (context, dryRun, step, totalSteps) => InstallBesqlAsync(context, dryRun, step, totalSteps));
        _features["httpclient"] = new(
            "httpclient",
            "Adds a typed HttpClient starter service.",
            (context, dryRun, step, totalSteps) => InstallHttpClientAsync(context, dryRun, step, totalSteps));
        _features["refit"] = new(
            "refit",
            "Adds Refit with a starter API contract and registration notes.",
            (context, dryRun, step, totalSteps) => InstallRefitAsync(context, dryRun, step, totalSteps));
        _features["flurl"] = new(
            "flurl",
            "Adds Flurl.Http with a starter API client service.",
            (context, dryRun, step, totalSteps) => InstallFlurlAsync(context, dryRun, step, totalSteps));
        _features["test"] = new(
            "test",
            "Creates a sibling xUnit test project wired to the target Blazor app.",
            (context, dryRun, step, totalSteps) => InstallTestProjectAsync(context, dryRun, step, totalSteps));
    }

    private async Task<InstallResult> InstallTailwindAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "theme.css"),
            """
            :root {
              --background: oklch(1 0 0);
              --foreground: oklch(0.145 0 0);
              --card: oklch(1 0 0);
              --card-foreground: oklch(0.145 0 0);
              --popover: oklch(1 0 0);
              --popover-foreground: oklch(0.145 0 0);
              --primary: oklch(0.205 0 0);
              --primary-foreground: oklch(0.985 0 0);
              --secondary: oklch(0.97 0 0);
              --secondary-foreground: oklch(0.205 0 0);
              --muted: oklch(0.97 0 0);
              --muted-foreground: oklch(0.556 0 0);
              --accent: oklch(0.97 0 0);
              --accent-foreground: oklch(0.205 0 0);
              --destructive: oklch(0.577 0.245 27.325);
              --destructive-foreground: oklch(1 0 0);
              --border: oklch(0.922 0 0);
              --input: oklch(0.922 0 0);
              --ring: oklch(0.708 0 0);
              --radius: 0.625rem;
            }

            .dark {
              --background: oklch(0.145 0 0);
              --foreground: oklch(0.985 0 0);
              --card: oklch(0.205 0 0);
              --card-foreground: oklch(0.985 0 0);
              --popover: oklch(0.205 0 0);
              --popover-foreground: oklch(0.985 0 0);
              --primary: oklch(0.985 0 0);
              --primary-foreground: oklch(0.205 0 0);
              --secondary: oklch(0.269 0 0);
              --secondary-foreground: oklch(0.985 0 0);
              --muted: oklch(0.269 0 0);
              --muted-foreground: oklch(0.708 0 0);
              --accent: oklch(0.269 0 0);
              --accent-foreground: oklch(0.985 0 0);
              --destructive: oklch(0.577 0.245 27.325);
              --destructive-foreground: oklch(1 0 0);
              --border: oklch(0.269 0 0);
              --input: oklch(0.269 0 0);
              --ring: oklch(0.439 0 0);
            }

            * {
              border-color: var(--border);
            }

            body {
              background-color: var(--background);
              color: var(--foreground);
            }
            """,
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "app-input.css"),
            """
            @import 'tailwindcss';
            @import './theme.css';

            @source "../Pages";
            @source "../Shared";
            @source "../Components";

            @custom-variant dark (&:where(.dark, .dark *));

            @theme inline {
              --color-border: var(--border);
              --color-input: var(--input);
              --color-ring: var(--ring);
              --color-background: var(--background);
              --color-foreground: var(--foreground);
              --color-primary: var(--primary);
              --color-primary-foreground: var(--primary-foreground);
              --color-secondary: var(--secondary);
              --color-secondary-foreground: var(--secondary-foreground);
              --color-destructive: var(--destructive);
              --color-destructive-foreground: var(--destructive-foreground);
              --color-muted: var(--muted);
              --color-muted-foreground: var(--muted-foreground);
              --color-accent: var(--accent);
              --color-accent-foreground: var(--accent-foreground);
              --color-popover: var(--popover);
              --color-popover-foreground: var(--popover-foreground);
              --color-card: var(--card);
              --color-card-foreground: var(--card-foreground);
              --font-sans: var(--font-sans, ui-sans-serif, system-ui, sans-serif);
              --font-mono: var(--font-mono, ui-monospace, monospace);
              --radius-lg: var(--radius);
              --radius-md: calc(var(--radius) - 2px);
              --radius-sm: calc(var(--radius) - 4px);
            }
            """,
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "app.css"),
            """
            /* Generated by tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css */
            """,
            dryRun,
            operations);

        EnsureBuildTarget(
            context.ProjectFilePath,
            """
              <Target Name="BuildTailwindCSS" BeforeTargets="BeforeBuild">
                <Message Text="Building Tailwind CSS..." Importance="high" />
                <Exec Command="tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css" />
              </Target>
            """,
            dryRun,
            operations);

        EnsureStylesheetReference(context, """<link href="css/app.css" rel="stylesheet" />""", dryRun, operations);

        nextSteps.Add("Download `tailwindcss.exe` to the project root as described in the Blazor Blueprint docs.");
        nextSteps.Add("Run `tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css` once to generate the first stylesheet.");
        nextSteps.Add("Keep `css/app.css` referenced instead of the pre-built Blueprint CSS when you want Tailwind utilities in your own Razor files.");

        return await Task.FromResult(InstallResult.Completed(
            "Installed Tailwind v4 standalone setup.",
            operations,
            nextSteps));
    }

    private async Task<InstallResult> InstallBlueprintAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsureBlueprintPackagesAsync(context, dryRun, operations, step, totalSteps);
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureProgramSetup(context, dryRun, operations);
        EnsureImports(context, dryRun, operations);
        EnsurePortalHost(context, dryRun, operations);
        EnsureStylesheetReference(context, """<link href="css/theme.css" rel="stylesheet" />""", dryRun, operations);

        var prefersCustomTailwind = File.Exists(Path.Combine(context.ProjectDirectory, "wwwroot", "css", "app.css"));
        if (!prefersCustomTailwind)
        {
            EnsureStylesheetReference(
                context,
                """<link href="_content/BlazorBlueprint.Components/blazorblueprint.css" rel="stylesheet" />""",
                dryRun,
                operations);
        }

        nextSteps.Add("If `tailwindcss.exe` is not configured, keep the pre-built Blueprint stylesheet reference.");
        nextSteps.Add("If you installed the `tailwind` feature, keep `css/app.css` as your active stylesheet and regenerate it on build.");
        nextSteps.Add("You can now import and use Blazor Blueprint components in pages and layouts.");

        return InstallResult.Completed(
            "Installed Blazor Blueprint integration.",
            operations,
            nextSteps);
    }

    private Task<InstallResult> InstallAuthAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "DemoUser.cs"),
            """
            namespace {{PROJECT_NAME}}.Authentication;

            public sealed record DemoUser(string Id, string Email, string[] Roles);
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "DemoAuthenticationStateProvider.cs"),
            """
            using Microsoft.AspNetCore.Components.Authorization;
            using System.Security.Claims;

            namespace {{PROJECT_NAME}}.Authentication;

            public sealed class DemoAuthenticationStateProvider : AuthenticationStateProvider
            {
                private DemoUser _currentUser = new("1", "demo@local", ["Admin"]);

                public override Task<AuthenticationState> GetAuthenticationStateAsync()
                {
                    var identity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, _currentUser.Id),
                        new Claim(ClaimTypes.Name, _currentUser.Email),
                        .. _currentUser.Roles.Select(static role => new Claim(ClaimTypes.Role, role))
                    ],
                    authenticationType: "BlazrDemo");

                    return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
                }

                public void SignOut()
                {
                    _currentUser = new DemoUser(string.Empty, string.Empty, []);
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "ServiceCollectionExtensions.cs"),
            """
            using Microsoft.AspNetCore.Components.Authorization;

            namespace {{PROJECT_NAME}}.Authentication;

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection AddBlazrDemoAuthentication(this IServiceCollection services)
                {
                    services.AddAuthorizationCore();
                    services.AddScoped<DemoAuthenticationStateProvider>();
                    services.AddScoped<AuthenticationStateProvider>(static provider =>
                        provider.GetRequiredService<DemoAuthenticationStateProvider>());

                    return services;
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "README.md"),
            """
            # Blazr Auth Starter

            Register the services in `Program.cs`:

            ```csharp
            builder.Services.AddBlazrDemoAuthentication();
            ```

            Wrap your app with `CascadingAuthenticationState` and use `AuthorizeView` where needed.
            Replace the demo provider with your real identity source when you move past the prototype stage.
            """,
            dryRun,
            operations);

        nextSteps.Add("Add `using " + context.ProjectName + ".Authentication;` to your `Program.cs`.");
        nextSteps.Add("Call `builder.Services.AddBlazrDemoAuthentication();` during app startup.");
        nextSteps.Add("Wrap your router in `CascadingAuthenticationState` and replace the demo provider when you connect real auth.");

        return Task.FromResult(InstallResult.Completed(
            "Installed authentication starter files.",
            operations,
            nextSteps));
    }

    private async Task<InstallResult> InstallPwaAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Bit.Bswup");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureImportsWithLines(
            context,
            dryRun,
            operations,
            "@using Bit.Bswup");

        var blazorScript = context.ProjectType == BlazorProjectType.Wasm
            ? "_framework/blazor.webassembly.js"
            : "_framework/blazor.web.js";

        EnsureBlazorAutostartDisabled(context, dryRun, operations, blazorScript);
        EnsureScriptReference(
            context,
            $$"""<script src="_content/Bit.Bswup/bit-bswup.progress.js"></script>""",
            dryRun,
            operations);
        EnsureScriptReference(
            context,
            $$"""<script src="_content/Bit.Bswup/bit-bswup.js" scope="/" log="verbose" sw="service-worker.js" blazorScript="{{blazorScript}}"></script>""",
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "service-worker.js"),
            """
            self.defaultUrl = '/';
            self.externalAssets = [{ url: '/' }];
            self.serverHandledUrls = [/\/api\//, /\/swagger\//];
            self.serverRenderedUrls = [/\/privacy$/];
            self.enableDiagnostics = true;

            self.importScripts('_content/Bit.Bswup/bit-bswup.sw.js');
            """,
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "service-worker.published.js"),
            """
            self.defaultUrl = '/';
            self.externalAssets = [{ url: '/' }];
            self.serverHandledUrls = [/\/api\//, /\/swagger\//];
            self.serverRenderedUrls = [/\/privacy$/];
            self.enableDiagnostics = true;

            self.importScripts('_content/Bit.Bswup/bit-bswup.sw.js');
            """,
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            ResolveComponentPath(context.ProjectDirectory, "BswupProgressHost.razor"),
            """
            <div class="blazr-bswup-host">
                <BswupProgress AppContainer="#app" AutoReload="false" ShowLogs="true" />
                <button id="bit-bswup-reload" style="display:none">Update available, click to reload</button>
            </div>
            """,
            dryRun,
            operations);

        EnsureLayoutHost(context, dryRun, operations, "<BswupProgressHost />");

        nextSteps.Add("Build and publish once to verify `service-worker.published.js` is emitted with the app.");
        nextSteps.Add("If your app root container is not `#app`, update `BswupProgressHost.razor` to match the real selector.");
        nextSteps.Add("Adjust `serverHandledUrls`, `serverRenderedUrls`, and other Bswup settings in `wwwroot/service-worker.js` as your app grows.");

        return InstallResult.Completed(
            "Installed Bit.Bswup PWA starter.",
            operations,
            nextSteps);
    }

    private async Task<InstallResult> InstallBesqlAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Bit.Besql");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureScriptReference(
            context,
            """<script src="_content/Bit.Besql/bit-besql.js"></script>""",
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "AppDbContext.cs"),
            """
            using Microsoft.EntityFrameworkCore;

            namespace {{PROJECT_NAME}}.Data;

            public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
            {
                public DbSet<TodoItem> Todos => Set<TodoItem>();
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "TodoItem.cs"),
            """
            namespace {{PROJECT_NAME}}.Data;

            public sealed class TodoItem
            {
                public int Id { get; set; }
                public string Title { get; set; } = string.Empty;
                public bool IsDone { get; set; }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "ServiceCollectionExtensions.cs"),
            """
            namespace {{PROJECT_NAME}}.Data;

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection Add{{PROJECT_NAME}}Data(this IServiceCollection services)
                {
                    services.AddBesqlDbContextFactory<AppDbContext>();
                    return services;
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Data",
            "builder.Services.Add" + context.ProjectName + "Data();");

        nextSteps.Add("Inject `IDbContextFactory<AppDbContext>` where needed and use it like the standard EF Core SQLite provider.");

        if (context.ProjectType != BlazorProjectType.Wasm)
        {
            nextSteps.Insert(0, "Bit.Besql is mainly intended for Blazor WebAssembly/browser storage. Verify this setup carefully for non-WASM projects.");
        }

        return InstallResult.Completed(
            "Installed Bit.Besql starter.",
            operations,
            nextSteps);
    }

    private async Task<InstallResult> InstallHttpClientAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ApiClient.cs"),
            """
            namespace {{PROJECT_NAME}}.Http;

            public sealed class ApiClient(HttpClient httpClient)
            {
                public async Task<string> GetHealthAsync(CancellationToken cancellationToken = default)
                {
                    return await httpClient.GetStringAsync("health", cancellationToken);
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            """
            namespace {{PROJECT_NAME}}.Http;

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection Add{{PROJECT_NAME}}Http(this IServiceCollection services, Uri baseAddress)
                {
                    services.AddHttpClient<ApiClient>(client => client.BaseAddress = baseAddress);
                    return services;
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Http(new Uri(\"https://api.example.com/\"));");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return await Task.FromResult(InstallResult.Completed(
            "Installed built-in HttpClient starter.",
            operations,
            nextSteps));
    }

    private async Task<InstallResult> InstallRefitAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Refit.HttpClientFactory");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "IExampleApi.cs"),
            """
            using Refit;

            namespace {{PROJECT_NAME}}.Http;

            public interface IExampleApi
            {
                [Get("/health")]
                Task<string> GetHealthAsync(CancellationToken cancellationToken = default);
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            """
            using Refit;

            namespace {{PROJECT_NAME}}.Http;

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection Add{{PROJECT_NAME}}Refit(this IServiceCollection services, Uri baseAddress)
                {
                    services.AddRefitClient<IExampleApi>()
                        .ConfigureHttpClient(client => client.BaseAddress = baseAddress);

                    return services;
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Refit(new Uri(\"https://api.example.com/\"));");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return InstallResult.Completed(
            "Installed Refit starter.",
            operations,
            nextSteps);
    }

    private async Task<InstallResult> InstallFlurlAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Flurl.Http");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ApiClient.cs"),
            """
            using Flurl.Http;

            namespace {{PROJECT_NAME}}.Http;

            public sealed class ApiClient(string baseUrl)
            {
                public Task<string> GetHealthAsync(CancellationToken cancellationToken = default)
                {
                    return baseUrl.AppendPathSegment("health").GetStringAsync(cancellationToken: cancellationToken);
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            """
            namespace {{PROJECT_NAME}}.Http;

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection Add{{PROJECT_NAME}}Flurl(this IServiceCollection services, string baseUrl)
                {
                    services.AddSingleton(new ApiClient(baseUrl));
                    return services;
                }
            }
            """.Replace("{{PROJECT_NAME}}", context.ProjectName, StringComparison.Ordinal),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Flurl(\"https://api.example.com/\");");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return InstallResult.Completed(
            "Installed Flurl starter.",
            operations,
            nextSteps);
    }

    private Task<InstallResult> InstallTestProjectAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();
        var testProjectName = $"{context.ProjectName}.Tests";
        var testProjectDirectory = Path.Combine(Directory.GetParent(context.ProjectDirectory)?.FullName ?? context.ProjectDirectory, testProjectName);

        WriteFile(
            testProjectDirectory,
            $"{testProjectName}.csproj",
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <IsPackable>false</IsPackable>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="coverlet.collector" Version="6.0.4" />
                <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
                <PackageReference Include="xunit" Version="2.9.3" />
                <PackageReference Include="xunit.runner.visualstudio" Version="3.1.3" />
              </ItemGroup>

              <ItemGroup>
                <ProjectReference Include="..\{{context.ProjectName}}\{{context.ProjectName}}.csproj" />
              </ItemGroup>

            </Project>
            """,
            dryRun,
            operations);

        WriteFile(
            testProjectDirectory,
            "SmokeTests.cs",
            $$"""
            namespace {{testProjectName}};

            public sealed class SmokeTests
            {
                [Fact]
                public void AppAssembly_Loads()
                {
                    var assemblyName = typeof(global::Program).Assembly.GetName().Name;
                    Assert.Equal("{{context.ProjectName}}", assemblyName);
                }
            }
            """,
            dryRun,
            operations);

        nextSteps.Add($"Run `dotnet restore {testProjectName}` after the project is created.");
        nextSteps.Add($"Add `{testProjectName}/{testProjectName}.csproj` to your solution if you keep a `.sln` file.");
        nextSteps.Add("Add bUnit or Playwright later if you want component or end-to-end test coverage.");

        return Task.FromResult(InstallResult.Completed(
            "Created xUnit starter project.",
            operations,
            nextSteps));
    }

    private Task<InstallResult> EnsureBlueprintPackagesAsync(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        int step,
        int totalSteps)
    {
        return EnsurePackagesAsync(
            context,
            dryRun,
            operations,
            step,
            totalSteps,
            "BlazorBlueprint.Components",
            "BlazorBlueprint.Icons.Lucide");
    }

    private async Task<InstallResult> EnsurePackagesAsync(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        int step,
        int totalSteps,
        params string[] packages)
    {
        foreach (var package in packages)
        {
            operations.Add(new FileOperation("package", package));

            if (dryRun)
            {
                continue;
            }

            var result = await commandExecutionService.RunAsync(
                "dotnet",
                $"add \"{context.ProjectFilePath}\" package {package}",
                context.ProjectDirectory,
                step,
                totalSteps,
                $"Installing package {package}");

            if (!result.Success)
            {
                return InstallResult.Failed($"Package install failed for '{package}': {result.Message}");
            }
        }

        return InstallResult.Completed("Packages installed.", operations, Array.Empty<string>());
    }

    private static void EnsureProgramSetup(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            "BlazorBlueprint.Components",
            "builder.Services.AddBlazorBlueprintComponents();");
    }

    private static void EnsureImports(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureImportsWithLines(
            context,
            dryRun,
            operations,
            "@using BlazorBlueprint.Components",
            "@using BlazorBlueprint.Icons.Lucide.Components",
            "@using BlazorBlueprint.Icons.Lucide.Data");
    }

    private static void EnsurePortalHost(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureLayoutHost(context, dryRun, operations, "<BbPortalHost />");
    }

    private static void EnsureStylesheetReference(
        ProjectContext context,
        string linkMarkup,
        bool dryRun,
        List<FileOperation> operations)
    {
        var appPath = FindFirstExistingFile(
            context.ProjectDirectory,
            "App.razor",
            Path.Combine("Components", "App.razor"));

        if (appPath is not null)
        {
            var content = File.ReadAllText(appPath);
            if (content.Contains(linkMarkup, StringComparison.Ordinal))
            {
                return;
            }

            var updated = linkMarkup + Environment.NewLine + content;
            operations.Add(new FileOperation("update", appPath));

            if (!dryRun)
            {
                File.WriteAllText(appPath, updated, Encoding.UTF8);
            }

            return;
        }

        var indexPath = FindFirstExistingFile(context.ProjectDirectory, Path.Combine("wwwroot", "index.html"));
        if (indexPath is null)
        {
            return;
        }

        var indexContent = File.ReadAllText(indexPath);
        if (indexContent.Contains(linkMarkup, StringComparison.Ordinal))
        {
            return;
        }

        var updatedIndex = indexContent.Replace(
            "</head>",
            $"    {linkMarkup}{Environment.NewLine}</head>",
            StringComparison.OrdinalIgnoreCase);

        operations.Add(new FileOperation("update", indexPath));

        if (!dryRun)
        {
            File.WriteAllText(indexPath, updatedIndex, Encoding.UTF8);
        }
    }

    private static void EnsureScriptReference(
        ProjectContext context,
        string scriptMarkup,
        bool dryRun,
        List<FileOperation> operations)
    {
        foreach (var path in GetHostDocumentPaths(context))
        {
            var fullPath = Path.Combine(context.ProjectDirectory, path);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var content = File.ReadAllText(fullPath);
            if (content.Contains(scriptMarkup, StringComparison.Ordinal))
            {
                return;
            }

            var updated = content.TrimEnd() + Environment.NewLine + scriptMarkup + Environment.NewLine;
            operations.Add(new FileOperation("update", fullPath));
            if (!dryRun)
            {
                File.WriteAllText(fullPath, updated, Encoding.UTF8);
            }

            return;
        }
    }

    private static void EnsureBlazorAutostartDisabled(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        string blazorScriptPath)
    {
        foreach (var path in GetHostDocumentPaths(context))
        {
            var fullPath = Path.Combine(context.ProjectDirectory, path);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var content = File.ReadAllText(fullPath);
            var source = $"""<script src="{blazorScriptPath}"></script>""";
            var target = $"""<script src="{blazorScriptPath}" autostart="false"></script>""";

            if (!content.Contains(source, StringComparison.Ordinal) || content.Contains(target, StringComparison.Ordinal))
            {
                continue;
            }

            operations.Add(new FileOperation("update", fullPath));
            if (!dryRun)
            {
                File.WriteAllText(fullPath, content.Replace(source, target, StringComparison.Ordinal), Encoding.UTF8);
            }

            return;
        }
    }

    private static IReadOnlyList<string> GetHostDocumentPaths(ProjectContext context)
    {
        return context.ProjectType switch
        {
            BlazorProjectType.Server => new[]
            {
                Path.Combine("Pages", "_Host.cshtml"),
                "_Host.cshtml"
            },
            _ => new[]
            {
                Path.Combine("wwwroot", "index.html"),
                "App.razor",
                Path.Combine("Components", "App.razor")
            }
        };
    }

    private static void EnsureBuildTarget(
        string projectFilePath,
        string targetXml,
        bool dryRun,
        List<FileOperation> operations)
    {
        var content = File.ReadAllText(projectFilePath);
        if (content.Contains("Name=\"BuildTailwindCSS\"", StringComparison.Ordinal))
        {
            return;
        }

        var projectEndTag = "</Project>";
        var updated = content.Replace(projectEndTag, Environment.NewLine + targetXml + Environment.NewLine + projectEndTag, StringComparison.Ordinal);

        operations.Add(new FileOperation("update", projectFilePath));
        if (!dryRun)
        {
            File.WriteAllText(projectFilePath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureProgramRegistration(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        string usingDirective,
        string registrationLine)
    {
        var programPath = Path.Combine(context.ProjectDirectory, "Program.cs");
        if (!File.Exists(programPath))
        {
            return;
        }

        var content = File.ReadAllText(programPath);
        var updated = content;

        updated = EnsureUsingDirective(updated, usingDirective);
        updated = EnsureLineNearBuild(updated, registrationLine);

        if (updated == content)
        {
            return;
        }

        operations.Add(new FileOperation("update", programPath));
        if (!dryRun)
        {
            File.WriteAllText(programPath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureImportsWithLines(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        params string[] lines)
    {
        var importsPath = FindFirstExistingFile(
            context.ProjectDirectory,
            "_Imports.razor",
            Path.Combine("Components", "_Imports.razor"));

        if (importsPath is null)
        {
            importsPath = Path.Combine(context.ProjectDirectory, "_Imports.razor");
        }

        var content = File.Exists(importsPath) ? File.ReadAllText(importsPath) : string.Empty;
        var updated = content;

        foreach (var line in lines)
        {
            updated = EnsureAppendedLine(updated, line);
        }

        if (updated == content)
        {
            return;
        }

        operations.Add(new FileOperation(File.Exists(importsPath) ? "update" : "create", importsPath));
        if (!dryRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(importsPath)!);
            File.WriteAllText(importsPath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureLayoutHost(ProjectContext context, bool dryRun, List<FileOperation> operations, string markup)
    {
        var layoutPath = FindFirstExistingFile(
            context.ProjectDirectory,
            Path.Combine("Components", "Layout", "MainLayout.razor"),
            Path.Combine("Shared", "MainLayout.razor"),
            "MainLayout.razor");

        if (layoutPath is null)
        {
            return;
        }

        var content = File.ReadAllText(layoutPath);
        if (content.Contains(markup, StringComparison.Ordinal))
        {
            return;
        }

        var updated = content.TrimEnd() + Environment.NewLine + Environment.NewLine + markup + Environment.NewLine;
        operations.Add(new FileOperation("update", layoutPath));

        if (!dryRun)
        {
            File.WriteAllText(layoutPath, updated, Encoding.UTF8);
        }
    }

    private static string ResolveComponentPath(string projectDirectory, string fileName)
    {
        var componentsShared = Path.Combine(projectDirectory, "Components", "Shared");
        if (Directory.Exists(componentsShared))
        {
            return Path.Combine("Components", "Shared", fileName);
        }

        var shared = Path.Combine(projectDirectory, "Shared");
        if (Directory.Exists(shared))
        {
            return Path.Combine("Shared", fileName);
        }

        return fileName;
    }

    private static void WriteFile(
        string rootDirectory,
        string relativePath,
        string content,
        bool dryRun,
        List<FileOperation> operations)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath);
        operations.Add(new FileOperation(File.Exists(fullPath) ? "update" : "create", fullPath));

        if (dryRun)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, NormalizeLineEndings(content), Encoding.UTF8);
    }

    private static string? FindFirstExistingFile(string rootDirectory, params string[] relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            var fullPath = Path.Combine(rootDirectory, relativePath);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string EnsureUsingDirective(string content, string usingDirective)
    {
        var line = $"using {usingDirective};";
        if (content.Contains(line, StringComparison.Ordinal))
        {
            return content;
        }

        return line + Environment.NewLine + content;
    }

    private static string EnsureLineAfterMatch(string content, string match, string lineToInsert)
    {
        if (content.Contains(lineToInsert, StringComparison.Ordinal))
        {
            return content;
        }

        var index = content.IndexOf(match, StringComparison.Ordinal);
        if (index < 0)
        {
            return content;
        }

        var insertionIndex = content.IndexOf(';', index);
        if (insertionIndex < 0)
        {
            return content;
        }

        insertionIndex += 1;
        return content.Insert(insertionIndex, Environment.NewLine + lineToInsert);
    }

    private static string EnsureLineNearBuild(string content, string lineToInsert)
    {
        if (content.Contains(lineToInsert, StringComparison.Ordinal))
        {
            return content;
        }

        const string buildLine = "var app = builder.Build();";
        var buildIndex = content.IndexOf(buildLine, StringComparison.Ordinal);
        if (buildIndex >= 0)
        {
            return content.Insert(buildIndex, lineToInsert + Environment.NewLine);
        }

        return EnsureLineAfterMatch(content, "builder.Services.AddRazorComponents()", lineToInsert);
    }

    private static string EnsureAppendedLine(string content, string line)
    {
        if (content.Contains(line, StringComparison.Ordinal))
        {
            return content;
        }

        if (!string.IsNullOrWhiteSpace(content) && !content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            content += Environment.NewLine;
        }

        return content + line + Environment.NewLine;
    }

    private static string NormalizeLineEndings(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", Environment.NewLine, StringComparison.Ordinal);
}
