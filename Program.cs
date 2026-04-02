using Blazr.Commands;
using Blazr.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.CommandLine;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
builder.Services.AddSingleton<CommandExecutionService>();
builder.Services.AddSingleton<TemplateProvider>();
builder.Services.AddSingleton<FeatureCatalog>();
builder.Services.AddSingleton<ProjectLocator>();
builder.Services.AddSingleton<ManifestService>();
builder.Services.AddSingleton<FeatureInstaller>();
builder.Services.AddSingleton<BlueprintTemplateService>();

using var host = builder.Build();

var services = host.Services;
var catalog = services.GetRequiredService<FeatureCatalog>();
var installer = services.GetRequiredService<FeatureInstaller>();
var locator = services.GetRequiredService<ProjectLocator>();
var manifest = services.GetRequiredService<ManifestService>();
var templateService = services.GetRequiredService<BlueprintTemplateService>();

var rootCommand = new RootCommand("Blazor ecosystem CLI for installing common app capabilities.");

rootCommand.AddCommand(ListCommandFactory.Create(catalog, locator, manifest));
rootCommand.AddCommand(AddCommandFactory.Create(catalog, installer));
rootCommand.AddCommand(NewCommandFactory.Create(templateService));

return await rootCommand.InvokeAsync(args);
