using System.Collections.Generic;
using Azure.API.Models.WorkItems;
using System.IO;

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
        if (node == null) return;

        var all = new List<string>();
        CollectNodePaths(node, all);

        IEnumerable<string> toAdd = all.Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(p => !paths.Contains(p, StringComparer.OrdinalIgnoreCase));
        paths.AddRange(toAdd);
    }

    private static void CollectNodePaths(ClassificationNode node, List<string> collector)
    {
        if (node == null) return;

        var raw = node.Path ?? string.Empty;
        if (raw.StartsWith(Path.DirectorySeparatorChar)) raw = raw.Substring(1);

        var segments = raw.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 2)
        {
            var workItemPath = segments[0];
            if (segments.Length > 2)
            {
                workItemPath = Path.Combine(new[] { workItemPath }.Concat(segments.Skip(2)).ToArray());
            }
            collector.Add(workItemPath);
        }
        else if (segments.Length == 1)
        {
            collector.Add(segments[0]);
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