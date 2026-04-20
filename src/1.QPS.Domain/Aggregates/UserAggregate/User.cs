using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.UserAggregate;

public class User : AggregateRoot
{
    public Guid MerchantId { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string RealName { get; private set; }
    public bool IsActive { get; private set; }

    protected User() { }

    public User(Guid merchantId, string username, string passwordHash, string realName)
    {
        MerchantId = merchantId;
        Username = username;
        PasswordHash = passwordHash;
        RealName = realName;
        IsActive = true;
    }

    public void Update(string realName)
    {
        RealName = realName;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void Activate() { IsActive = true; }
    public void Deactivate() { IsActive = false; }
}