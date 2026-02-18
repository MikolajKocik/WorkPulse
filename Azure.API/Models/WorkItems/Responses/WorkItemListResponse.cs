using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.API.Models.WorkItems.Responses;

namespace Azure.API.Models.WorkItems.Responses;

/// <summary>
/// Response model for batch work items list from Azure DevOps API.
/// </summary>
public class WorkItemListResponse
{
    /// <summary>
    /// The count of work items returned.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The list of work items.
    /// </summary>
    public List<WorkItemResponse> Value { get; set; }
}