using System.Text.RegularExpressions;
using System.Xml.Linq;

using Xunit;

namespace XboxLag.Adapters.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void ProductionReferencesFollowLayerDirectionWithoutCycles()
    {
        string root = FindRoot();
        Dictionary<string, string[]> graph = Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)
            .ToDictionary(p => Path.GetFileNameWithoutExtension(p),
                p => XDocument.Load(p).Descendants("ProjectReference")
                    .Select(e => Path.GetFileNameWithoutExtension(e.Attribute("Include")!.Value.Replace('\\', '/'))).ToArray(),
                StringComparer.Ordinal);
        Assert.Equal(4, graph.Count);
        Assert.Empty(graph["XboxLag.Domain"]);
        Assert.Equal(["XboxLag.Domain"], graph["XboxLag.Application"]);
        Assert.DoesNotContain("XboxLag.Adapters", graph["XboxLag.Infrastructure"]);
        foreach (string project in graph.Keys)
        {
            Visit(project, graph, []);
        }
    }

    [Fact]
    public void FunctionalNamespacesMatchSourceDirectories()
    {
        string root = FindRoot();
        string[] modules = ["Games", "Routing", "Relays", "Measurements", "Sessions"];
        foreach (string assembly in new[] { "XboxLag.Domain", "XboxLag.Application", "XboxLag.Adapters" })
        {
            string directory = Path.Combine(root, "src", assembly);
            foreach (string file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(directory, file);
                string[] parts = relative.Split(Path.DirectorySeparatorChar);
                if (parts[0] is "obj" or "bin") { continue; }
                Assert.Contains(parts[0], modules);
                string expected = assembly + "." + string.Join('.', parts[..^1]);
                Assert.Matches("namespace " + Regex.Escape(expected) + ";", File.ReadAllText(file));
            }
        }
    }

    private static void Visit(string project, Dictionary<string, string[]> graph, HashSet<string> path)
    {
        Assert.True(path.Add(project), $"Circular reference at {project}");
        Assert.True(graph.TryGetValue(project, out string[]? references));
        foreach (string reference in references)
        {
            Visit(reference, graph, path);
        }
        path.Remove(project);
    }

    private static string FindRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "XboxLag.slnx")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
