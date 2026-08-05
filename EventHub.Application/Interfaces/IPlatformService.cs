using EventHub.Application.DTOs.Platform;

namespace EventHub.Application.Interfaces;

public interface IPlatformService
{
    Task<PlatformStatsDto> GetStatsAsync();
}
