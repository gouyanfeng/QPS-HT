using System;
using System.ComponentModel.DataAnnotations;

namespace QPS.Application.Contracts.Merchants;

public class MerchantDto
{
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string PhoneNumber { get; set; }
    
    public int PowerOffDelayMinutes { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
}