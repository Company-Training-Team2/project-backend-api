using EventHub.Application.DTOs.Common;
using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.Interfaces;

public interface IWorkPostService
{
    Task<PagedResultDto<WorkPostSummaryDto>> SearchAsync(WorkPostSearchQuery query);

    Task<WorkPostDetailDto> GetDetailAsync(int workPostId);

    /// <summary>Top-rated approved WorkPosts, optionally biased to a city. Used by the Home dashboard.</summary>
    Task<IEnumerable<WorkPostSummaryDto>> GetFeaturedAsync(string? city, int take = 6);
}
