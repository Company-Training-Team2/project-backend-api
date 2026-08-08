using System.Security.Claims;
using EventHub.Application.DTOs.Expense;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Module 4 – Budget & Expenses.
///
/// Routes (matches the audit's API Contracts table):
///   GET    /api/events/{id}/budget     → budget metrics summary (Total / Spent / Remaining / Category Breakdown)
///   GET    /api/events/{id}/expenses   → fetch the expense ledger
///   POST   /api/events/{id}/expenses   → add a manual expense entry
///   PUT    /api/expenses/{id}          → modify a manual expense entry
///   DELETE /api/expenses/{id}          → drop a manual expense entry
/// </summary>
[ApiController]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // ── GET /api/events/{id}/budget ───────────────────────────────────────────

    [HttpGet("api/events/{id}/budget")]
    public async Task<ActionResult<BudgetSummaryDto>> GetBudget(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _expenseService.GetBudgetSummaryAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return Ok(result);
    }

    // ── GET /api/events/{id}/expenses ─────────────────────────────────────────

    [HttpGet("api/events/{id}/expenses")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _expenseService.GetExpensesAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return Ok(result);
    }

    // ── POST /api/events/{id}/expenses ────────────────────────────────────────

    [HttpPost("api/events/{id}/expenses")]
    public async Task<ActionResult<ExpenseDto>> AddExpense(int id, [FromBody] CreateExpenseRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _expenseService.AddExpenseAsync(id, userId, request);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return CreatedAtAction(nameof(GetExpenses), new { id }, result);
    }

    // ── PUT /api/expenses/{id} ────────────────────────────────────────────────

    [HttpPut("api/expenses/{id}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _expenseService.UpdateExpenseAsync(id, userId, request);

        if (result == null)
            return NotFound(new { message = "Expense not found, access denied, or it is a system-generated entry that can't be edited manually." });

        return Ok(result);
    }

    // ── DELETE /api/expenses/{id} ─────────────────────────────────────────────

    [HttpDelete("api/expenses/{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = GetCurrentUserId();

        var deleted = await _expenseService.DeleteExpenseAsync(id, userId);

        if (!deleted)
            return NotFound(new { message = "Expense not found, access denied, or it is a system-generated entry that can't be deleted manually." });

        return NoContent();
    }
}
