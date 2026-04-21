using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Customer : AggregateRoot
{
    public string OpenId { get; private set; }
    public string Phone { get; private set; }
    public string Nickname { get; private set; }
    public string AvatarUrl { get; private set; }

    protected Customer() { }

    public Customer(string openId, string phone, string nickname, string avatarUrl)
    {
        OpenId = openId;
        Phone = phone;
        Nickname = nickname;
        AvatarUrl = avatarUrl;
    }

    public void Update(string phone, string nickname, string avatarUrl)
    {
        Phone = phone;
        Nickname = nickname;
        AvatarUrl = avatarUrl;
    }
}