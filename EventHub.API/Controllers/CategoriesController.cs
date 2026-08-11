using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// GET /api/categories — public, unauthenticated. Real Categories table,
/// replaces the frontend's lib/mock/categories.ts fixture for anywhere a
/// real WorkPost.CategoryId is needed (e.g. the vendor Add/Edit Service form).
/// </summary>
[ApiController]
[Route("api/categories")]
[AllowAnonymous]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }
}
