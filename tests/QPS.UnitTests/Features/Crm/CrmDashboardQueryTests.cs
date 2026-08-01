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
        var followingCustomer = CreateCustomer(ownerUserId, "Following Customer");
        followingCustomer.UpdateFollowSummary(DateTime.Now, "INTERESTED", DateTime.Now.AddDays(1));
        followingCustomer.UpdateStatus("FOLLOWING", "");
        dbContext.CrmHerbBases.Add(followingCustomer);
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
        var firstHighGradePendingCustomer = CreateCustomer(ownerUserId, "First High Grade Pending Customer", "A");
        var secondHighGradePendingCustomer = CreateCustomer(ownerUserId, "Second High Grade Pending Customer", "高");
        var interestedCustomer = CreateCustomer(ownerUserId, "Interested Customer");
        interestedCustomer.UpdateStatus("INTERESTED", "");

        dbContext.CrmHerbBases.AddRange(firstHighGradePendingCustomer, secondHighGradePendingCustomer, interestedCustomer);
        await dbContext.SaveChangesAsync();

        var handler = new GetCrmDashboardHandler(
            dbContext,
            new TestCurrentUserService(ownerUserId.ToString()));

        var result = await handler.Handle(new GetCrmDashboardQuery(), CancellationToken.None);

        Assert.Equal(1, result.Metrics.HighIntentCustomerCount);
    }

    private static CrmHerbBase CreateCustomer(Guid ownerUserId, string name, string grade = "B")
    {
        return CrmHerbBase.Create(
            name,
            grade,
            80,
            "Gansu",
            "Dingxi",
            "Longxi",
            "Test address",
            null,
            null,
            "MANUAL",
            null,
            ownerUserId,
            "");
    }
}
