using Spectre.Console;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Blazr.Services;

internal sealed record CommandExecutionResult(bool Success, string Message);

internal sealed class CommandExecutionService(IAnsiConsole console)
{
    public async Task<CommandExecutionResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        int step,
        int totalSteps,
        string title)
    {
        console.MarkupLine($"[grey]{Markup.Escape($"{fileName} {arguments}")}[/]");

        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return new CommandExecutionResult(false, "Failed to start process.");
        }

        var latestLine = new ConcurrentQueue<string>();
        var outputTask = ReadLinesAsync(process.StandardOutput, latestLine);
        var errorTask = ReadLinesAsync(process.StandardError, latestLine);

        await console.Progress()
            .AutoRefresh(true)
            .HideCompleted(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            ])
            .StartAsync(async progressContext =>
            {
                var task = progressContext.AddTask($"[blue]{Markup.Escape(BuildDescription(title, step, totalSteps, null))}[/]");
                task.IsIndeterminate = true;

                while (!process.HasExited)
                {
                    if (TryGetLatest(latestLine, out var line))
                    {
                        task.Description = $"[blue]{Markup.Escape(BuildDescription(title, step, totalSteps, line))}[/]";
                    }

                    await Task.Delay(120);
                }

                await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

                if (TryGetLatest(latestLine, out var finalLine))
                {
                    task.Description = $"[green]{Markup.Escape(BuildDescription(title, step, totalSteps, finalLine))}[/]";
                }
                else
                {
                    task.Description = $"[green]{Markup.Escape(BuildDescription(title, step, totalSteps, "completed"))}[/]";
                }

                task.IsIndeterminate = false;
                task.Value = task.MaxValue;
            });

        if (process.ExitCode != 0)
        {
            return new CommandExecutionResult(
                false,
                $"Command failed with exit code {process.ExitCode}: {fileName} {arguments}");
        }

        console.WriteLine();
        return new CommandExecutionResult(true, string.Empty);
    }

    private static async Task ReadLinesAsync(StreamReader reader, ConcurrentQueue<string> latestLine)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            latestLine.Enqueue(line.Trim());

            while (latestLine.Count > 3)
            {
                latestLine.TryDequeue(out _);
            }
        }
    }

    private static bool TryGetLatest(ConcurrentQueue<string> latestLine, out string line)
    {
        line = string.Empty;
        if (latestLine.IsEmpty)
        {
            return false;
        }

        line = latestLine.LastOrDefault() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(line);
    }

    private static string BuildDescription(string title, int step, int totalSteps, string? statusLine)
    {
        var prefix = totalSteps > 0 ? $"[{step}/{totalSteps}] {title}" : title;
        if (string.IsNullOrWhiteSpace(statusLine))
        {
            return prefix;
        }

        var compact = statusLine.Length > 70 ? statusLine[..67] + "..." : statusLine;
        return $"{prefix} | {compact}";
    }
}
