using EventHub.Application.DTOs.Common;
using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.Interfaces;

public interface IWorkPostService
{
    Task<PagedResultDto<WorkPostSummaryDto>> SearchAsync(WorkPostSearchQuery query);

    Task<WorkPostDetailDto> GetDetailAsync(int workPostId);

    /// <summary>Top-rated approved WorkPosts, optionally biased to a city. Used by the Home dashboard.</summary>
    Task<IEnumerable<WorkPostSummaryDto>> GetFeaturedAsync(string? city, int take = 6);

    /// <summary>Card-level projections for the given WorkPost IDs. Used by the Favorites dashboard (audit Module 9).</summary>
    Task<IEnumerable<WorkPostSummaryDto>> GetSummariesByIdsAsync(IEnumerable<int> workPostIds);
}
