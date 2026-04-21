namespace QPS.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    Guid MerchantId { get; }
    Guid? ShopId { get; }
}