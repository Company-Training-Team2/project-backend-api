using EventHub.Application.DTOs.Messaging;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// Vendor&lt;-&gt;Customer direct messaging ("Contact Vendor"). A Conversation
/// always has exactly one Customer-role user and one Vendor-role user on
/// it, so every method here works from either side — GetMyConversationsAsync
/// figures out which side the caller is on per-row and returns the *other*
/// party's info.
/// </summary>
public class MessagingService : IMessagingService
{
    private readonly IUnitOfWork _unitOfWork;

    public MessagingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(int userId)
    {
        var conversations = await _unitOfWork.Repository<Conversation>()
            .Query()
            .Where(c => c.CustomerUserId == userId || c.VendorUserId == userId)
            .Include(c => c.CustomerUser).ThenInclude(u => u.CustomerProfile)
            .Include(c => c.VendorUser).ThenInclude(u => u.VendorProfile)
            .Include(c => c.WorkPost)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return conversations.Select(c => ToDto(c, userId));
    }

    public async Task<ConversationDto> CreateConversationAsync(int customerUserId, CreateConversationDto dto)
    {
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Include(w => w.VendorProfile)
            .FirstOrDefaultAsync(w => w.Id == dto.WorkPostId)
            ?? throw new InvalidOperationException("Listing not found.");

        var vendorUserId = workPost.VendorProfile.UserId;

        if (vendorUserId == customerUserId)
            throw new InvalidOperationException("You can't message your own listing.");

        // Find-or-create: repeated "Contact Vendor" clicks on the same
        // listing resume the existing thread instead of spawning a new one
        // every time.
        var existing = await _unitOfWork.Repository<Conversation>()
            .Query()
            .Include(c => c.CustomerUser).ThenInclude(u => u.CustomerProfile)
            .Include(c => c.VendorUser).ThenInclude(u => u.VendorProfile)
            .Include(c => c.WorkPost)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c =>
                c.CustomerUserId == customerUserId &&
                c.VendorUserId == vendorUserId &&
                c.WorkPostId == dto.WorkPostId);

        int conversationId;
        if (existing != null)
        {
            conversationId = existing.Id;
        }
        else
        {
            var conversation = new Conversation
            {
                CustomerUserId = customerUserId,
                VendorUserId = vendorUserId,
                WorkPostId = dto.WorkPostId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Conversation>().AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();
            conversationId = conversation.Id;
        }

        if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
        {
            var message = new ConversationMessage
            {
                ConversationId = conversationId,
                SenderUserId = customerUserId,
                Body = dto.InitialMessage,
                IsReadByCustomer = true,
                IsReadByVendor = false,
                SentAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ConversationMessage>().AddAsync(message);

            var toTouch = existing ?? await _unitOfWork.Repository<Conversation>().GetByIdAsync(conversationId)
                ?? throw new InvalidOperationException("Conversation not found.");
            toTouch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Conversation>().Update(toTouch);

            await _unitOfWork.SaveChangesAsync();
        }

        // Re-fetch with every navigation property populated for ToDto,
        // regardless of whether this was a find, a create, or a
        // create-then-message — simpler and safer than patching navigation
        // properties onto a partially-loaded in-memory instance.
        var full = await _unitOfWork.Repository<Conversation>()
            .Query()
            .Include(c => c.CustomerUser).ThenInclude(u => u.CustomerProfile)
            .Include(c => c.VendorUser).ThenInclude(u => u.VendorProfile)
            .Include(c => c.WorkPost)
            .Include(c => c.Messages)
            .FirstAsync(c => c.Id == conversationId);

        return ToDto(full, customerUserId);
    }

    public async Task<IEnumerable<ConversationMessageDto>> GetMessagesAsync(int conversationId, int userId)
    {
        var conversation = await _unitOfWork.Repository<Conversation>()
            .Query()
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new InvalidOperationException("Conversation not found.");

        if (conversation.CustomerUserId != userId && conversation.VendorUserId != userId)
            throw new InvalidOperationException("You don't have access to this conversation.");

        var isCustomer = conversation.CustomerUserId == userId;
        var unread = conversation.Messages
            .Where(m => m.SenderUserId != userId && (isCustomer ? !m.IsReadByCustomer : !m.IsReadByVendor))
            .ToList();
        if (unread.Count > 0)
        {
            foreach (var m in unread)
            {
                if (isCustomer) m.IsReadByCustomer = true; else m.IsReadByVendor = true;
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new ConversationMessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderUserId = m.SenderUserId,
                IsFromMe = m.SenderUserId == userId,
                Body = m.Body,
                SentAt = m.SentAt
            });
    }

    public async Task<ConversationMessageDto> SendMessageAsync(int conversationId, int userId, SendConversationMessageDto dto)
    {
        var conversation = await _unitOfWork.Repository<Conversation>().GetByIdAsync(conversationId)
            ?? throw new InvalidOperationException("Conversation not found.");

        if (conversation.CustomerUserId != userId && conversation.VendorUserId != userId)
            throw new InvalidOperationException("You don't have access to this conversation.");

        var isCustomer = conversation.CustomerUserId == userId;

        var message = new ConversationMessage
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Body = dto.Body,
            IsReadByCustomer = isCustomer,
            IsReadByVendor = !isCustomer,
            SentAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<ConversationMessage>().AddAsync(message);

        conversation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Conversation>().Update(conversation);

        await _unitOfWork.SaveChangesAsync();

        return new ConversationMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            IsFromMe = true,
            Body = message.Body,
            SentAt = message.SentAt
        };
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static ConversationDto ToDto(Conversation c, int viewerUserId)
    {
        var viewerIsCustomer = c.CustomerUserId == viewerUserId;
        var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        var unread = c.Messages.Count(m =>
            m.SenderUserId != viewerUserId && (viewerIsCustomer ? !m.IsReadByCustomer : !m.IsReadByVendor));

        return new ConversationDto
        {
            Id = c.Id,
            OtherPartyUserId = viewerIsCustomer ? c.VendorUserId : c.CustomerUserId,
            OtherPartyName = viewerIsCustomer
                ? (c.VendorUser?.VendorProfile?.BusinessName ?? "Vendor")
                : (c.CustomerUser?.CustomerProfile?.FullName ?? "Customer"),
            OtherPartyRole = viewerIsCustomer ? nameof(UserRole.Vendor) : nameof(UserRole.Customer),
            WorkPostId = c.WorkPostId,
            WorkPostTitle = c.WorkPost?.Title,
            LastMessageSnippet = lastMsg == null
                ? null
                : lastMsg.Body.Length > 80 ? lastMsg.Body[..80] + "…" : lastMsg.Body,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            UnreadCount = unread
        };
    }
}
