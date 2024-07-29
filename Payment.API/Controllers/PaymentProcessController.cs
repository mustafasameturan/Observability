using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ILogger = Serilog.ILogger;

namespace Payment.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PaymentProcessController : ControllerBase
{
    private readonly ILogger<PaymentProcessController> _logger;

    public PaymentProcessController(ILogger<PaymentProcessController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Create(PaymentCreateRequestDto request)
    {
        if (HttpContext.Request.Headers.TryGetValue("traceparent", out StringValues values))
        {
            Console.WriteLine($"traceParent: {values.First()}");
        }
        
        const decimal balance = 1000;

        if (request.TotalPrice > balance)
        {
            _logger.LogWarning("Yetersiz bakiye. orderCode: {@orderCode}", request.OrderCode);
            return BadRequest(ResponseDto<PaymentCreateResponseDto>.Fail(400, "Yetersiz bakiye!"));
        }

        _logger.LogInformation("Kart işlemi başarıyla gerçekleşmiştir. orderCode: {@orderCode}", request.OrderCode);
        return Ok(ResponseDto<PaymentCreateResponseDto>.Success(200, new PaymentCreateResponseDto()
        {
            Description = "Kart işlemi başarıyla gerçekleştirmiştir!"
        }));
    }
}