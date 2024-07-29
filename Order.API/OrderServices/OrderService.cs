using System.Diagnostics;
using System.Net;
using Common.Shared.DTOs;
using Common.Shared.Events;
using MassTransit;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.RedisServices;
using Order.API.StockServices;
using ILogger = Serilog.ILogger;

namespace Order.API.OrderServices;

public class OrderService
{
    private AppDbContext _dbContext;
    private readonly StockService _stockService;
    private readonly RedisService _redisService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext dbContext, StockService stockService, RedisService redisService, IPublishEndpoint publishEndpoint, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _stockService = stockService;
        _redisService = redisService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateRequestDto request)
    {
        #region Bu kod bloğu ile redis işleminin kaç saniye sürdüğü vs gibi trace dataları yazdırabiliriz fakat redisin kendi insurmentation paketi olduğundan bu koda gerek kalmıyor
        // using (var _ = ActivitySourceProvider.Source.StartActivity("Redis Request"))
        // {
        //     await _redisService.GetDb(0).StringSetAsync("userId", request.UserId);
        // }
        #endregion

        using (var redisActivity = ActivitySourceProvider.Source.StartActivity("RedisGetSetActivity"))
        {
            await _redisService.GetDb(0).StringSetAsync("userId", request.UserId);

            redisActivity.SetTag("userId", request.UserId);
            
            var redisUserId = _redisService.GetDb(0).StringGetAsync("UserId");    
        }
        
        // Program.cs aracılığı ile çalışan ana aktivitiye bağlı kısım
        Activity.Current?.SetTag("Asp.Net Core Span(insturmentation) tag1", "span value tag value");
        
        using var activity = ActivitySourceProvider.Source.StartActivity();
        activity?.SetBaggage("userId", request.UserId.ToString());
        activity?.AddEvent(new("Sipariş süreci başladı!"));

        var newOrder = new Order()
        {
            Created = DateTime.Now,
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Succeed,
            UserId = request.UserId,
            Items = request.OrderItems.Select(x => new OrderItem()
            {
                Count = x.Count,
                ProductId = x.ProductId,
                Price = x.Price
            }).ToList()
        };

        _dbContext.Orders.Add(newOrder);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Sipariş veritabanına kaydedildi. {@userId}", request.UserId);
            
        StockCheckAndPaymentProcessRequestDto stockRequest = new()
        {
            OrderCode = newOrder.OrderCode,
            OrderItems = request.OrderItems
        };

        var (isSuccess, failMessage) = await _stockService.CheckAndPaymentStart(stockRequest);

        if (!isSuccess)
        {
            return ResponseDto<OrderCreateResponseDto>.Fail(HttpStatusCode.InternalServerError.GetHashCode(), failMessage!);
        }
        
        activity?.AddEvent(new("Sipariş süreci tamamlandı!"));
        
        return ResponseDto<OrderCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(), new OrderCreateResponseDto() { Id = newOrder.Id });
    }
}