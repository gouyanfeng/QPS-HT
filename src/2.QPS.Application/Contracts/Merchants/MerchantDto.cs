using System;
using System.ComponentModel.DataAnnotations;

namespace QPS.Application.Contracts.Merchants;

public class MerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}