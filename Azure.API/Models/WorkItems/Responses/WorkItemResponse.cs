using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.Models.WorkItems.Responses;

/// <summary>
/// Response model for a created Work Item from Azure DevOps API.
/// </summary>
public class WorkItemResponse
{
    /// <summary>
    /// The ID of the work item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The revision number of the work item.
    /// </summary>
    public int Rev { get; set; }

    /// <summary>
    /// The fields of the work item.
    /// </summary>
    public Dictionary<string, object> Fields { get; set; }

    /// <summary>
    /// The URL of the work item.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The links related to the work item.
    /// </summary>
    public Links _links { get; set; }

    /// <summary>
    /// Links class for work item relations.
    /// </summary>
    public class Links
    {
        public Link Self { get; set; }
        public Link WorkItemUpdates { get; set; }
        public Link WorkItemRevisions { get; set; }
        public Link WorkItemHistory { get; set; }
        public Link Html { get; set; }
        public Link WorkItemType { get; set; }
        public Link Fields { get; set; }
    }

    /// <summary>
    /// Link class.
    /// </summary>
    public class Link
    {
        public string Href { get; set; }
    }
}