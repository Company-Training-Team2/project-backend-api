using EventHub.Application.DTOs.Home;

namespace EventHub.Application.Interfaces;

public interface IHomeService
{
    Task<HomeDashboardDto> GetDashboardAsync();
}
