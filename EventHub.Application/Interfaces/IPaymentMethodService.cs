using EventHub.Application.DTOs.Payment;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Module 8 – saved payment instruments (GET / POST / DELETE /api/payments/methods).
/// Scoped to the currently authenticated customer, same auth pattern as IPaymentService.
/// </summary>
public interface IPaymentMethodService
{
    Task<IEnumerable<SavedPaymentMethodDto>> GetMyPaymentMethodsAsync();

    Task<SavedPaymentMethodDto> AddPaymentMethodAsync(CreateSavedPaymentMethodRequest request);

    /// <summary>Returns false when the method doesn't exist or doesn't belong to the current customer.</summary>
    Task<bool> DeletePaymentMethodAsync(int id);
}
