using MediatR;
using Microsoft.Extensions.Logging;
using QPS.Application.Interfaces;
using QPS.Domain.Events;
using QPS.Infrastructure.Sms;

namespace QPS.Infrastructure.EventHandlers;

public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IDbContext _dbContext;
    private readonly ISmsService _smsService;
    private readonly ILogger<OrderPaidEventHandler> _logger;

    public OrderPaidEventHandler(IDbContext dbContext, ISmsService smsService, ILogger<OrderPaidEventHandler> logger)
    {
        _dbContext = dbContext;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // 获取订单信息
            var order = await _dbContext.Orders.FindAsync(notification.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning($"订单 {notification.OrderId} 不存在");
                return;
            }

            // 获取客户信息
            var customer = order.CustomerId.HasValue
                ? await _dbContext.Customers.FindAsync(order.CustomerId.Value, cancellationToken)
                : null;

            if (customer != null && !string.IsNullOrEmpty(customer.Phone))
            {
                // 发送支付成功短信
                var message = $"【QPS】您的订单 {order.OrderNo} 已支付成功，金额 {order.ActualAmount:F2} 元。感谢您的使用！";
                await _smsService.SendAsync(customer.Phone, message);

                _logger.LogInformation($"已向客户 {customer.Phone} 发送订单支付成功短信");
            }
            else
            {
                _logger.LogInformation($"订单 {order.OrderNo} 支付成功，但未绑定客户或客户无手机号");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理订单支付事件失败，订单ID: {notification.OrderId}");
            // 短信发送失败不影响主流程，不抛出异常
        }
    }
}