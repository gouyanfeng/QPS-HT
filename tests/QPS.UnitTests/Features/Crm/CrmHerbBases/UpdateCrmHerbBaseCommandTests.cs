using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmHerbBases;
using QPS.Domain.Entities.Crm;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmHerbBases;

public class UpdateCrmHerbBaseCommandTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSource_WhenSourcePlatformChanges()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var customer = CrmHerbBase.Create(
            "Codex Test Customer",
            "B",
            80,
            "Gansu",
            "Dingxi",
            "Longxi",
            "Test address",
            null,
            null,
            "BAIDU_MAP",
            1001,
            null,
            "Initial remark");
        dbContext.CrmHerbBases.Add(customer);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCrmHerbBaseHandler(dbContext);

        var result = await handler.Handle(new UpdateCrmHerbBaseCommand
        {
            Id = customer.Id,
            Request = new CrmHerbBaseUpdateRequest
            {
                HerbBaseName = "Codex Test Customer",
                MainProducts = new List<string> { "HUANG_QI" },
                Grade = "B",
                Score = 80,
                Province = "Gansu",
                City = "Dingxi",
                Area = "Longxi",
                Address = "Test address",
                SourcePlatform = "MANUAL",
                SourceId = 2002,
                Status = "PENDING",
                Remark = "Initial remark"
            }
        }, CancellationToken.None);

        var persistedCustomer = await dbContext.CrmHerbBases.SingleAsync(item => item.Id == customer.Id);
        Assert.True(result);
        Assert.Equal("MANUAL", persistedCustomer.SourcePlatform);
        Assert.Equal(2002, persistedCustomer.SourceId);
    }

}



