using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.WorkItems.Requests;

/// <summary>
/// Request model for creating a Work Item (Task or User Story).
/// All fields are nullable since user may not provide all values.
/// </summary>
public class WorkItemRequest
{
    /// <summary>
    /// Title of the work item (required in Azure DevOps).
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description of the work item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Assigned to (user identity).
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Priority (integer, e.g., 1-4).
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Tags (comma-separated string).
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Area Path.
    /// </summary>
    public string? AreaPath { get; set; }

    /// <summary>
    /// Iteration Path.
    /// </summary>
    public string? IterationPath { get; set; }
}