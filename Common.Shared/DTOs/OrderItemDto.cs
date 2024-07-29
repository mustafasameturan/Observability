namespace Common.Shared.DTOs;

public record OrderItemDto
{
    public int ProductId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
}