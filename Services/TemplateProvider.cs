using System.Reflection;

namespace Blazr.Services;

internal sealed class TemplateProvider
{
    private static readonly Assembly Assembly = typeof(TemplateProvider).Assembly;

    public string Load(string featureKey, string fileName)
    {
        var resourceName = $"Blazr.Templates.{featureKey}.{fileName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public string Load(string featureKey, string fileName, string projectName)
    {
        return Load(featureKey, fileName)
            .Replace("{{PROJECT_NAME}}", projectName, StringComparison.Ordinal);
    }

    public string Load(string featureKey, string fileName, string projectName, string testProjectName)
    {
        return Load(featureKey, fileName)
            .Replace("{{PROJECT_NAME}}", projectName, StringComparison.Ordinal)
            .Replace("{{TEST_PROJECT_NAME}}", testProjectName, StringComparison.Ordinal);
    }
}
