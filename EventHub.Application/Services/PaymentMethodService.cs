using System.Security.Claims;
using EventHub.Application.DTOs.Payment;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

/// <summary>
/// Module 8 – saved payment instruments. Fills the gap identified in the
/// audit's API Contracts table: /api/payments/methods (GET / POST / DELETE)
/// had no backing service or controller.
/// </summary>
public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentMethodService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<SavedPaymentMethodDto>> GetMyPaymentMethodsAsync()
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var methods = await _unitOfWork.Repository<SavedPaymentMethod>()
            .FindAsync(m => m.CustomerId == profile.Id);

        return methods
            .OrderByDescending(m => m.IsDefault)
            .ThenByDescending(m => m.Id)
            .Select(MapToDto);
    }

    public async Task<SavedPaymentMethodDto> AddPaymentMethodAsync(CreateSavedPaymentMethodRequest request)
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var method = new SavedPaymentMethod
        {
            CustomerId = profile.Id,
            Type = request.Type,
            MaskedNumber = request.MaskedNumber,
            CardHolderName = request.CardHolderName,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            GatewayToken = request.GatewayToken,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        // Only one default instrument per customer.
        if (method.IsDefault)
        {
            var existing = await _unitOfWork.Repository<SavedPaymentMethod>()
                .FindAsync(m => m.CustomerId == profile.Id && m.IsDefault);

            foreach (var m in existing)
            {
                m.IsDefault = false;
                _unitOfWork.Repository<SavedPaymentMethod>().Update(m);
            }
        }

        await _unitOfWork.Repository<SavedPaymentMethod>().AddAsync(method);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(method);
    }

    public async Task<bool> DeletePaymentMethodAsync(int id)
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var method = await _unitOfWork.Repository<SavedPaymentMethod>().GetByIdAsync(id);

        if (method is null || method.CustomerId != profile.Id)
            return false;

        _unitOfWork.Repository<SavedPaymentMethod>().Delete(method);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<CustomerProfile> GetCurrentCustomerProfileAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Customer profile not found.");

        return profile;
    }

    private static SavedPaymentMethodDto MapToDto(SavedPaymentMethod m) => new()
    {
        Id = m.Id,
        Type = m.Type,
        MaskedNumber = m.MaskedNumber,
        CardHolderName = m.CardHolderName,
        ExpiryMonth = m.ExpiryMonth,
        ExpiryYear = m.ExpiryYear,
        IsDefault = m.IsDefault
    };
}
