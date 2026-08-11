using EventHub.Application.DTOs.Category;

namespace EventHub.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
}
