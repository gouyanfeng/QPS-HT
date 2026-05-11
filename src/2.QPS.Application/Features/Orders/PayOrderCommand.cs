using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Orders;

/// <summary>
/// 支付订单命令
/// </summary>
public class PayOrderCommand : IRequest<bool>
{
    /// <summary>
    /// 订单ID
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 支付金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 支付方式（wechat/alipay/cash）
    /// </summary>
    public string PaymentMethod { get; set; }
}

/// <summary>
/// 支付订单处理器
/// </summary>
public class PayOrderHandler : IRequestHandler<PayOrderCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="currentUserService">当前用户服务</param>
    /// <param name="publisher">事件发布器</param>
    public PayOrderHandler(IDbContext dbContext, ICurrentUserService currentUserService, IPublisher publisher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    /// <summary>
    /// 处理支付订单请求
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        if (order == null || order.MerchantId != merchantId) return false;

        // 检查订单状态是否可以支付
        if (order.Status != OrderStatus.WaitingPayment) return false;

        // 支付订单
        order.Pay();

        // 更新订单金额
        order.GetType().GetProperty("OriginAmount")?.SetValue(order, request.Amount);
        order.GetType().GetProperty("ActualAmount")?.SetValue(order, request.Amount);
        order.GetType().GetProperty("PaymentMethod")?.SetValue(order, request.PaymentMethod);

        // 保存到数据库
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 发布订单支付成功事件
        await _publisher.Publish(new OrderPaidEvent(order.Id), cancellationToken);

        return true;
    }
}