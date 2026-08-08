using EventHub.Application.DTOs.Expense;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Module 4 – Budget & Expenses.
/// Authorization pattern: identical to ChecklistService — verify event
/// ownership via IEventService.EventBelongsToUserAsync, then operate on
/// Expense via the generic IUnitOfWork repository.
/// All methods accept the ASP.NET Identity userId (JWT NameIdentifier claim).
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// GET /api/events/{id}/budget — Total, Spent, Remaining, and Category
    /// Breakdown. Returns null when the event is not found or does not
    /// belong to the user.
    /// </summary>
    Task<BudgetSummaryDto?> GetBudgetSummaryAsync(int eventId, int userId);

    /// <summary>
    /// GET /api/events/{id}/expenses — full ledger (system-generated +
    /// manual entries), newest first. Returns null when the event is not
    /// found or does not belong to the user.
    /// </summary>
    Task<IEnumerable<ExpenseDto>?> GetExpensesAsync(int eventId, int userId);

    /// <summary>
    /// POST /api/events/{id}/expenses — manual out-of-pocket entry.
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<ExpenseDto?> AddExpenseAsync(int eventId, int userId, CreateExpenseRequest request);

    /// <summary>
    /// PUT /api/expenses/{id}. Returns null when the item is not found, does
    /// not belong to the user's event, or is a system-generated (booking-linked)
    /// entry that can't be manually edited.
    /// </summary>
    Task<ExpenseDto?> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseRequest request);

    /// <summary>
    /// DELETE /api/expenses/{id}. Returns false when the item is not found,
    /// does not belong to the user's event, or is system-generated.
    /// </summary>
    Task<bool> DeleteExpenseAsync(int expenseId, int userId);
}
