using EventHub.Application.DTOs.Messaging;

namespace EventHub.Application.Interfaces;

public interface IMessagingService
{
    Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(int userId);

    Task<ConversationDto> CreateConversationAsync(int customerUserId, CreateConversationDto dto);

    Task<IEnumerable<ConversationMessageDto>> GetMessagesAsync(int conversationId, int userId);

    Task<ConversationMessageDto> SendMessageAsync(int conversationId, int userId, SendConversationMessageDto dto);
}
