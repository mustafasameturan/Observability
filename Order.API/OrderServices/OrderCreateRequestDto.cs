using Common.Shared.DTOs;

namespace Order.API.OrderServices;

public record OrderCreateRequestDto
{
    public int UserId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = null!;
}