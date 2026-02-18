using System.Collections.Generic;
using Azure.API.Models.WorkItems;

namespace Azure.API.Utils;

public static class AzureBoardUtils
{
    /// <summary>
    /// Flattens the node path.
    /// </summary>
    /// <param name="node">The node to flatten.</param>
    /// <param name="paths">The list of paths.</param>
    public static void FlattenNodePath(ClassificationNode node, List<string> paths)
    {
        // Provide the path. The root Path usually includes the Project Name.
        // e.g. "\Project\Area\SubArea"
        // Clean it up slightly if it starts with \
        if (node == null) return;

        // Collect all display paths (without project prefix)
        var all = new List<string>();
        CollectNodePaths(node, all);

        // Filter out any path that is a strict prefix of another path (keep deeper paths only)
        var filtered = all.Where(p => !all.Any(other => other != p && other.StartsWith(p + "\\", StringComparison.OrdinalIgnoreCase)))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();

        // Append to provided paths preserving order
        foreach (var fp in filtered)
        {
            if (!paths.Contains(fp)) paths.Add(fp);
        }
    }
    
    private static void CollectNodePaths(ClassificationNode node, List<string> collector)
    {
        if (node == null) return;

        var p = node.Path ?? string.Empty;
        if (p.StartsWith("\\")) p = p.Substring(1);

        var segments = p.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 1)
        {
            var display = string.Join("\\", segments.Skip(1));
            if (!string.IsNullOrEmpty(display)) collector.Add(display);
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                CollectNodePaths(child, collector);
            }
        }
    }
}