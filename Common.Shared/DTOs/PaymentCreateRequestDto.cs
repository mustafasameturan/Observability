namespace Common.Shared.DTOs;

public record PaymentCreateRequestDto
{
    public string OrderCode { get; set; } = null!;
    public decimal TotalPrice { get; set; }
}