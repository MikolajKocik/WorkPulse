using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.API.Services.Interfaces;

public interface IAzureBoardService
{
    Task<List<string>> GetClassificationNodesAsync(string structureGroup, CancellationToken ct = default);
}
