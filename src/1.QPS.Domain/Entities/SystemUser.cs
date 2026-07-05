using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class SystemUser : AggregateRoot
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string RealName { get; private set; }
    public bool IsActive { get; private set; }

    private SystemUser(string username, string passwordHash, string realName)
    {
        Username = username;
        PasswordHash = passwordHash;
        RealName = realName;
        IsActive = true;
    }

    public static SystemUser Create(string username, string passwordHash, string realName)
    {
        return new SystemUser(username, passwordHash, realName);
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