using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmContacts;
using QPS.Domain.Entities.Crm;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmContacts;

public class CrmContactCommandTests
{
    [Fact]
    public async Task Create_ShouldPromoteFirstContactToPrimary_WhenCustomerHasNoPrimaryContact()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var customer = CreateCustomer();
        dbContext.CrmHerbBases.Add(customer);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCrmContactHandler(dbContext);

        var result = await handler.Handle(new CreateCrmContactCommand
        {
            CustomerId = customer.Id,
            Request = new CrmContactCreateRequest
            {
                ContactName = "First Contact",
                Phone = "13800000001",
                PhoneType = "MOBILE",
                RoleName = "OWNER",
                IsPrimary = false
            }
        }, CancellationToken.None);

        var persistedCustomer = await dbContext.CrmHerbBases.SingleAsync(item => item.Id == customer.Id);
        var persistedContact = await dbContext.CrmContacts.SingleAsync();
        Assert.True(result);
        Assert.True(persistedContact.IsPrimary);
        Assert.Equal("First Contact", persistedCustomer.PrimaryContactName);
        Assert.Equal("13800000001", persistedCustomer.PrimaryContactPhone);
    }

    [Fact]
    public async Task UpdateStatus_ShouldPromoteOldestValidContact_WhenPrimaryContactBecomesInvalid()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var customer = CreateCustomer();
        var primary = CrmContact.Create(
            "CRM_HERB_BASE",
            customer.Id,
            "Primary Contact",
            "13800000001",
            "MOBILE",
            "",
            "OWNER",
            true,
            "");
        var replacement = CrmContact.Create(
            "CRM_HERB_BASE",
            customer.Id,
            "Replacement Contact",
            "13800000002",
            "MOBILE",
            "",
            "PURCHASE",
            false,
            "");
        primary.CreatedAt = DateTime.UtcNow.AddMinutes(-10);
        replacement.CreatedAt = DateTime.UtcNow.AddMinutes(-5);
        customer.UpdatePrimaryContact(primary.ContactName, primary.Phone);
        dbContext.CrmHerbBases.Add(customer);
        dbContext.CrmContacts.AddRange(primary, replacement);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCrmContactStatusHandler(dbContext);

        await handler.Handle(new UpdateCrmContactStatusCommand
        {
            Id = primary.Id,
            Request = new CrmContactStatusRequest
            {
                Status = "INVALID",
                Remark = "Wrong number"
            }
        }, CancellationToken.None);

        Assert.False(primary.IsPrimary);
        Assert.True(replacement.IsPrimary);
        Assert.Equal("INVALID", primary.Status);
        Assert.Equal("Replacement Contact", customer.PrimaryContactName);
        Assert.Equal("13800000002", customer.PrimaryContactPhone);
    }

    private static CrmHerbBase CreateCustomer()
    {
        return CrmHerbBase.Create(
            "Contact Test Customer",
            "B",
            80,
            "Gansu",
            "Dingxi",
            "Longxi",
            "Test address",
            null,
            null,
            "MANUAL",
            null,
            null,
            "Remark");
    }
}




