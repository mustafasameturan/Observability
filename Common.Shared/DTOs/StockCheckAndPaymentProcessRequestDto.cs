namespace Common.Shared.DTOs;

public record StockCheckAndPaymentProcessRequestDto
{
    public string OrderCode { get; set; } = null!;
    public List<OrderItemDto> OrderItems { get; set; } = null!;
}