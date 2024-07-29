using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Stock.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;

    public StockController(StockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost]
    public async Task<IActionResult> CheckAndPaymentStart(StockCheckAndPaymentProcessRequestDto request)
    {
        //Header datası alındı ve incelendi.
        var header = HttpContext.Request.Headers;
        
        var result = await _stockService.CheckAndPaymentProcess(request);
        return new ObjectResult(result) { StatusCode = result.StatusCode };
    }
}