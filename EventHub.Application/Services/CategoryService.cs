using EventHub.Application.DTOs.Category;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// GET /api/categories — public, unauthenticated. Backs the Category
/// dropdown on the vendor Add/Edit Service form (and any future
/// customer-facing category filter) with the real Categories table instead
/// of the frontend's lib/mock/categories.ts fixture.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _unitOfWork.Repository<Category>().Query()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }
}
