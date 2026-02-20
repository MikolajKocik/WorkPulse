using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.Config;

/// <summary>
/// POCO settings class for work item connection to azure devops
/// </summary>
public class WorkItemUri
{
    public required string Organization { get; set; }
    public required string Project { get; set; }
    public required List<string> SupportedTypes { get; set; }
    public required string DefaultType { get; set; }
    public required string Pat { get; set; }
}