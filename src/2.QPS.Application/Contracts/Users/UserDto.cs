using System;

namespace QPS.Application.Contracts.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Username { get; set; }
    public string RealName { get; set; }
    public bool IsActive { get; set; }
}