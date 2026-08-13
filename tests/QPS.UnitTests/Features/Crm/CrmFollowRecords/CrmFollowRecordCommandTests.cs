using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmFollowRecords;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmFollowRecords;

public class CrmFollowRecordCommandTests
{
    [Fact]
    public async Task Create_ShouldUpdateCustomerFollowSummary_WhenFollowRecordIsCreated()
    {
        var operatorUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(operatorUserId.ToString()));
        var subject = CreateSubject(operatorUserId, "Follow Test Subject");
        var herbBase = CrmHerbBase.Create(
            "Follow Test Customer",
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
        herbBase.SetHerbBaseSubject(subject.Id);
        var contact = CrmContact.Create(
            "CRM_HERB_BASE_SUBJECT",
            subject.Id,
            "Follow Contact",
            "13800000001",
            "MOBILE",
            "",
            "OWNER",
            true,
            "");
        dbContext.CrmHerbBaseSubjects.Add(subject);
        dbContext.CrmHerbBases.Add(herbBase);
        dbContext.CrmContacts.Add(contact);
        await dbContext.SaveChangesAsync();

        var nextFollowAt = DateTime.Now.AddDays(3);
        var handler = new CreateCrmFollowRecordHandler(
            dbContext,
            new TestCurrentUserService(operatorUserId.ToString()),
            TestDbContextFactory.CreatePublisher());

        var result = await handler.Handle(new CreateCrmFollowRecordCommand
        {
            HerbBaseSubjectId = subject.Id,
            Request = new CrmFollowRecordCreateRequest
            {
                HerbBaseId = herbBase.Id,
                ContactId = contact.Id,
                FollowType = "PHONE",
                FollowResult = "INTERESTED",
                IntentLevel = "A",
                Content = "Customer is interested",
                NextFollowAt = nextFollowAt
            }
        }, CancellationToken.None);

        var persistedRecord = await dbContext.CrmFollowRecords.SingleAsync();
        Assert.True(result);
        Assert.Equal(contact.Id, persistedRecord.ContactId);
        Assert.Equal(operatorUserId, persistedRecord.OperatorUserId);
        Assert.Equal(subject.Id, persistedRecord.HerbBaseSubjectId);
        Assert.Equal(herbBase.Id, persistedRecord.HerbBaseId);
        Assert.Equal("INTERESTED", subject.LastFollowResult);
        Assert.Equal(nextFollowAt, subject.NextFollowAt);
        Assert.Equal("INTERESTED", subject.Status);
    }

    [Fact]
    public async Task Create_ShouldRejectVendorContactForCustomer()
    {
        var operatorUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(operatorUserId.ToString()));
        var subject = CreateSubject(operatorUserId, "Follow Test Subject");
        var contact = CrmContact.Create(
            "CRM_VENDOR",
            Guid.NewGuid(),
            "Vendor Contact",
            "13900000000",
            "MOBILE",
            "",
            "PURCHASE",
            true,
            "");

        dbContext.CrmHerbBaseSubjects.Add(subject);
        dbContext.CrmContacts.Add(contact);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCrmFollowRecordHandler(
            dbContext,
            new TestCurrentUserService(operatorUserId.ToString()),
            TestDbContextFactory.CreatePublisher());

        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new CreateCrmFollowRecordCommand
        {
            HerbBaseSubjectId = subject.Id,
            Request = new CrmFollowRecordCreateRequest
            {
                ContactId = contact.Id,
                FollowType = "PHONE",
                FollowResult = "CONNECTED",
                IntentLevel = "MEDIUM",
                Content = "Should reject vendor contact",
                NextFollowAt = null
            }
        }, CancellationToken.None));
    }

    private static CrmHerbBaseSubject CreateSubject(Guid ownerUserId, string name)
        => CrmHerbBaseSubject.Create(
            name,
            name,
            "UNKNOWN",
            ownerUserId,
            "PENDING",
            "B",
            80,
            "Remark");
}




