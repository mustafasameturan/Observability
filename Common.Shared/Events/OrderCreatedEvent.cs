namespace Common.Shared.Events;

public record OrderCreatedEvent
{
    public string OrderCode { get; set; } = null!;
}