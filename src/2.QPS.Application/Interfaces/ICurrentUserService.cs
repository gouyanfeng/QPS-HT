namespace QPS.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    Guid MerchantId { get; }
    Guid? ShopId { get; }
}