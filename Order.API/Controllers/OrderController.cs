using Common.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.OrderServices;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderController(OrderService orderService, IPublishEndpoint publishEndpoint)
    {
        _orderService = orderService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateRequestDto requestDto)
    {
        var result = await _orderService.CreateAsync(requestDto);
        
        #region ThirdParty api istek örneği

        // var httpClient = new HttpClient();
        //
        // var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
        //
        // await response.Content.ReadAsStringAsync();

        #endregion
        
        return new ObjectResult(result) { StatusCode = result.StatusCode };
        
        #region Exception örneği için hazırlandı.

        // var a = 10;
        // var b = 0;
        // var c = a / b;

        #endregion
    }

    [HttpGet]
    public async Task<IActionResult> SendOrderCreatedEvent()
    {
        // Kuyruğa mesaj gönder
        await _publishEndpoint.Publish(new OrderCreatedEvent() { OrderCode = Guid.NewGuid().ToString() });
        return Ok();
    }
}