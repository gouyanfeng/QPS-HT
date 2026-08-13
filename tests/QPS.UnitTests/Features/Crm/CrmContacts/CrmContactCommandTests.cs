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
        var subject = CreateSubject();
        dbContext.CrmHerbBaseSubjects.Add(subject);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCrmContactHandler(dbContext, TestDbContextFactory.CreatePublisher());

        var result = await handler.Handle(new CreateCrmContactCommand
        {
            HerbBaseSubjectId = subject.Id,
            Request = new CrmContactCreateRequest
            {
                ContactName = "First Contact",
                Phone = "13800000001",
                PhoneType = "MOBILE",
                RoleName = "OWNER",
                IsPrimary = false
            }
        }, CancellationToken.None);

        var persistedSubject = await dbContext.CrmHerbBaseSubjects.SingleAsync(item => item.Id == subject.Id);
        var persistedContact = await dbContext.CrmContacts.SingleAsync();
        Assert.True(result);
        Assert.True(persistedContact.IsPrimary);
        Assert.Equal("First Contact", persistedSubject.PrimaryContactName);
        Assert.Equal("13800000001", persistedSubject.PrimaryContactPhone);
    }

    [Fact]
    public async Task UpdateStatus_ShouldPromoteOldestValidContact_WhenPrimaryContactBecomesInvalid()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var subject = CreateSubject();
        var primary = CrmContact.Create(
            "CRM_HERB_BASE_SUBJECT",
            subject.Id,
            "Primary Contact",
            "13800000001",
            "MOBILE",
            "",
            "OWNER",
            true,
            "");
        var replacement = CrmContact.Create(
            "CRM_HERB_BASE_SUBJECT",
            subject.Id,
            "Replacement Contact",
            "13800000002",
            "MOBILE",
            "",
            "PURCHASE",
            false,
            "");
        primary.CreatedAt = DateTime.UtcNow.AddMinutes(-10);
        replacement.CreatedAt = DateTime.UtcNow.AddMinutes(-5);
        subject.UpdatePrimaryContact(primary.ContactName, primary.Phone);
        dbContext.CrmHerbBaseSubjects.Add(subject);
        dbContext.CrmContacts.AddRange(primary, replacement);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCrmContactStatusHandler(dbContext, TestDbContextFactory.CreatePublisher());

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
        Assert.Equal("Replacement Contact", subject.PrimaryContactName);
        Assert.Equal("13800000002", subject.PrimaryContactPhone);
    }

    private static CrmHerbBaseSubject CreateSubject()
        => CrmHerbBaseSubject.Create(
            "Contact Test Subject",
            "Contact Test Base",
            "UNKNOWN",
            null,
            "PENDING",
            "B",
            80,
            "Remark");
}
