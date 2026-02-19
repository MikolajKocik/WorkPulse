using Azure.API.Models.WorkItems.Requests;
using Azure.API.Models.WorkItems.Responses;

namespace Azure.API.Services.Interfaces
{
    public interface IWorkItemService
    {
        Task<WorkItemListResponse> GetWorkItemListAsync(int[] ids, string[]? fields = null, CancellationToken ct = default);

        Task<WorkItemResponse> GetWorkItemInfoAsync(int id);

        Task<string> CreateWorkItemAsync(WorkItemRequest request, string? type = null, CancellationToken ct = default);

        Task<int[]> QueryWorkItemIdsAsync(string? filter = null, CancellationToken ct = default);

        Task<bool> DeleteWorkItemAsync(int id);

        Task<string> UpdateWorkItemAsync(int id, WorkItemRequest request, CancellationToken ct = default);
    }
}
