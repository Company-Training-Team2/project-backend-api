using EventHub.Application.DTOs.Expense;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

/// <summary>
/// Module 4 – Budget & Expenses.
///
/// Fills the "Architectural Gap Identified" from the audit: the Expense
/// entity/table already existed (used internally by PaymentService to
/// auto-generate ledger entries for paid bookings — the Hybrid Model's
/// system side), but there was no service/controller exposing it to
/// customers for the manual-entry side or the budget summary endpoint.
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventService _eventService;

    public ExpenseService(IUnitOfWork unitOfWork, IEventService eventService)
    {
        _unitOfWork = unitOfWork;
        _eventService = eventService;
    }

    // ─── GET budget summary ─────────────────────────────────────────────────

    public async Task<BudgetSummaryDto?> GetBudgetSummaryAsync(int eventId, int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var evt = await _unitOfWork.Repository<Event>().GetByIdAsync(eventId);
        if (evt is null)
            return null;

        var expenses = (await _unitOfWork.Repository<Expense>()
            .FindAsync(e => e.EventId == eventId)).ToList();

        var spent = expenses.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Amount);
        var pending = expenses.Where(e => e.Status != ExpenseStatus.Paid).Sum(e => e.Amount);

        var breakdown = expenses
            .Where(e => e.Status == ExpenseStatus.Paid)
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdownItemDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount),
                Percentage = spent > 0 ? (double)(g.Sum(e => e.Amount) / spent) * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        return new BudgetSummaryDto
        {
            EventId = eventId,
            TotalBudget = evt.TotalBudget,
            SpentBudget = spent,
            RemainingBudget = evt.TotalBudget - spent,
            PendingBudget = pending,
            CategoryBreakdown = breakdown
        };
    }

    // ─── GET expenses ────────────────────────────────────────────────────────

    public async Task<IEnumerable<ExpenseDto>?> GetExpensesAsync(int eventId, int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var expenses = await _unitOfWork.Repository<Expense>()
            .FindAsync(e => e.EventId == eventId);

        return expenses
            .OrderByDescending(e => e.Date)
            .Select(MapToDto)
            .ToList();
    }

    // ─── CREATE (manual entry) ───────────────────────────────────────────────

    public async Task<ExpenseDto?> AddExpenseAsync(int eventId, int userId, CreateExpenseRequest request)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var expense = new Expense
        {
            EventId = eventId,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            Status = ParseStatus(request.Status),
            Date = request.Date ?? DateTime.UtcNow,
            BookingId = null, // manual entries are never linked to a booking
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Expense>().AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(expense);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public async Task<ExpenseDto?> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseRequest request)
    {
        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(expenseId);
        if (expense is null)
            return null;

        var owned = await _eventService.EventBelongsToUserAsync(expense.EventId, userId);
        if (!owned)
            return null;

        // System-generated entries mirror a Payment 1:1 (unique BookingId
        // index) — editing them here would desync them from the actual
        // payment record, so only manual entries are editable.
        if (expense.BookingId.HasValue)
            return null;

        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.Status = ParseStatus(request.Status);
        expense.Date = request.Date;
        expense.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Expense>().Update(expense);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(expense);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    public async Task<bool> DeleteExpenseAsync(int expenseId, int userId)
    {
        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(expenseId);
        if (expense is null)
            return false;

        var owned = await _eventService.EventBelongsToUserAsync(expense.EventId, userId);
        if (!owned)
            return false;

        if (expense.BookingId.HasValue)
            return false;

        _unitOfWork.Repository<Expense>().Delete(expense);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ExpenseStatus ParseStatus(string? status) =>
        Enum.TryParse<ExpenseStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : ExpenseStatus.Pending;

    private static ExpenseDto MapToDto(Expense e) => new()
    {
        Id = e.Id,
        EventId = e.EventId,
        Category = e.Category,
        Description = e.Description,
        Amount = e.Amount,
        Status = e.Status.ToString(),
        Date = e.Date,
        BookingId = e.BookingId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
