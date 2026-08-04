using Microsoft.EntityFrameworkCore;
using QPS.Application.Features.Crm;
using QPS.Domain.Entities.Crm;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm;

public class CrmDashboardQueryTests
{
    [Fact]
    public async Task Handle_ShouldBuildFollowFunnelFromCustomerStatusOnly()
    {
        var ownerUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(ownerUserId.ToString()));
        var followingSubject = CreateSubject(ownerUserId, "Following Subject");
        followingSubject.UpdateFollowSummary(DateTime.Now, "CONNECTED", DateTime.Now.AddDays(1));
        dbContext.CrmHerbBaseSubjects.Add(followingSubject);
        await dbContext.SaveChangesAsync();

        var handler = new GetCrmDashboardHandler(
            dbContext,
            new TestCurrentUserService(ownerUserId.ToString()));

        var result = await handler.Handle(new GetCrmDashboardQuery(), CancellationToken.None);

        Assert.Equal(1, result.FollowFunnel.Single(item => item.Code == "FOLLOWING").Value);
        Assert.Equal(0, result.FollowFunnel.Single(item => item.Code == "INTERESTED").Value);
    }

    [Fact]
    public async Task Handle_ShouldCountHighIntentCustomersFromInterestedStatus()
    {
        var ownerUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(ownerUserId.ToString()));
        var firstHighGradePendingSubject = CreateSubject(ownerUserId, "First High Grade Pending Subject", "A");
        var secondHighGradePendingSubject = CreateSubject(ownerUserId, "Second High Grade Pending Subject", "高");
        var interestedSubject = CreateSubject(ownerUserId, "Interested Subject");
        interestedSubject.UpdateFollowSummary(DateTime.Now, "INTERESTED", null);

        dbContext.CrmHerbBaseSubjects.AddRange(firstHighGradePendingSubject, secondHighGradePendingSubject, interestedSubject);
        await dbContext.SaveChangesAsync();

        var handler = new GetCrmDashboardHandler(
            dbContext,
            new TestCurrentUserService(ownerUserId.ToString()));

        var result = await handler.Handle(new GetCrmDashboardQuery(), CancellationToken.None);

        Assert.Equal(1, result.Metrics.HighIntentSubjectCount);
    }

    private static CrmHerbBaseSubject CreateSubject(Guid ownerUserId, string name, string grade = "B")
    {
        return CrmHerbBaseSubject.Create(
            name,
            name,
            "UNKNOWN",
            ownerUserId,
            "PENDING",
            grade,
            80,
            "");
    }
}
