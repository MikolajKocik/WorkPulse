using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.Config;

/// <summary>
/// POCO settings class for work item connection to azure devops
/// </summary>
public class WorkItemURI
{
    public string Organization { get; set; }
    public string Project { get; set; }
    public List<string> SupportedTypes { get; set; }
    public string DefaultType { get; set; }
    public string Pat { get; set; }
}