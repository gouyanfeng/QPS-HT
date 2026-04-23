using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class User : AggregateRoot
{
    public Guid MerchantId { get; private set; }
    public Guid RoleId { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string RealName { get; private set; }
    public bool IsActive { get; private set; }

    private User(string username, string passwordHash, string realName, Guid roleId)
    {
        MerchantId = Guid.Empty;
        RoleId = roleId;
        Username = username;
        PasswordHash = passwordHash;
        RealName = realName;
        IsActive = true;
    }

    public static User Create(string username, string passwordHash, string realName, Guid roleId)
    {
        return new User(username, passwordHash, realName, roleId);
    }

    public void Update(string realName, Guid roleId)
    {
        RealName = realName;
        RoleId = roleId;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void Activate() { IsActive = true; }
    public void Deactivate() { IsActive = false; }
}