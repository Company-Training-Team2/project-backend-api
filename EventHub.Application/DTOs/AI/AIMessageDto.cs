using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.AI;

public class AIMessageDto
{
    public AIMessageSender Sender { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
