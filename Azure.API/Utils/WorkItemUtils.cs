using Azure.API.Config;
using Azure.API.Models.WorkItems.Requests;

namespace Azure.API.Utils;

/// <summary>
/// Utility class for Work Item operations.
/// </summary>
public static class WorkItemUtils
{
    /// <summary>
    /// Creates a list of JSON Patch operations from a WorkItemRequest.
    /// </summary>
    /// <param name="request">The work item request data.</param>
    /// <returns>List of operations for Azure DevOps API.</returns>
    public static List<object> RequestOperations(WorkItemRequest request)
    {
        var operations = new List<object>();

        // Always add Title (required)
        operations.Add(new
        {
            op = "add",
            path = "/fields/System.Title",
            value = request.Title
        });

        // Add optional fields if provided
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/System.Description",
                value = request.Description
            });
        }

        if (!string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/System.AssignedTo",
                value = request.AssignedTo
            });
        }

        if (request.Priority.HasValue)
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/Microsoft.VSTS.Common.Priority",
                value = request.Priority.Value
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Tags))
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/System.Tags",
                value = request.Tags
            });
        }

        if (!string.IsNullOrWhiteSpace(request.AreaPath))
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/System.AreaPath",
                value = request.AreaPath
            });
        }

        if (!string.IsNullOrWhiteSpace(request.IterationPath))
        {
            operations.Add(new
            {
                op = "add",
                path = "/fields/System.IterationPath",
                value = request.IterationPath
            });
        }

        return operations;
    }

    public static string[] WorkItemConnectionParameters(WorkItemURI wi)
    {
        string organization = wi.Organization;
        string project = wi.Project;

        return [organization, project];
    }
}