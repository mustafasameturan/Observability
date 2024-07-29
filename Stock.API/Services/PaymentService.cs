using Common.Shared.DTOs;

namespace Stock.API.Services;

public class PaymentService
{
    private readonly HttpClient _client;

    public PaymentService(HttpClient client)
    {
        _client = client;
    }

    public async Task<(bool isSuccess, string? failMessage)> CreatePaymentProcess(PaymentCreateRequestDto request)
    {
        var response =
            await _client.PostAsJsonAsync<PaymentCreateRequestDto>("api/PaymentProcess/Create",
                request);

        var responseContent =
            await response.Content.ReadFromJsonAsync<ResponseDto<PaymentCreateResponseDto>>();

        return response.IsSuccessStatusCode 
            ? (true, null) 
            : (false, responseContent!.Errors!.First());
    }
}