using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.WorkItems.Requests;

/// <summary>
/// Request model for batch getting work items.
/// </summary>
public class WorkItemBatchRequest
{
    /// <summary>
    /// The requested work item IDs (max 200).
    /// </summary>
    public int[] Ids { get; set; }

    /// <summary>
    /// The requested fields (e.g., ["System.Id", "System.Title"]).
    /// </summary>
    public string[] Fields { get; set; }

    /// <summary>
    /// The expand parameters (optional, default None).
    /// </summary>
    public string? Expand { get; set; } = "Fields";

    /// <summary>
    /// Error policy (optional, default Omit).
    /// </summary>
    public string? ErrorPolicy { get; set; } = "Omit";
}