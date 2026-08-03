using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmVendors;
using QPS.Domain.Entities.System;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmVendors;

public class CrmVendorCommandTests
{
    [Fact]
    public async Task Create_ShouldAddDefaultTransferRecord()
    {
        var operatorUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(operatorUserId.ToString()));
        var owner = SystemUser.Create("owner", "password", "Owner", Guid.NewGuid());
        dbContext.SystemUsers.Add(owner);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCrmVendorHandler(dbContext, new TestCurrentUserService(operatorUserId.ToString()));

        var result = await handler.Handle(new CreateCrmVendorCommand
        {
            Request = new CrmVendorCreateRequest
            {
                VendorName = "Default Transfer Vendor",
                PriorityLevel = "High",
                OwnerUserId = owner.Id,
                LatestPurchasePlanName = "采购计划",
                Remark = "Created by unit test"
            }
        }, CancellationToken.None);

        var vendor = await dbContext.CrmVendors.SingleAsync(item => item.VendorName == "Default Transfer Vendor");
        var transferRecord = await dbContext.CrmTransferRecords.SingleAsync();

        Assert.True(result);
        Assert.Equal("CRM_VENDOR", transferRecord.EntityType);
        Assert.Equal(vendor.Id, transferRecord.EntityId);
        Assert.Null(transferRecord.FromOwnerUserId);
        Assert.Equal(owner.Id, transferRecord.ToOwnerUserId);
        Assert.Equal(operatorUserId, transferRecord.OperatorUserId);
        Assert.Equal("Created by unit test", transferRecord.Remark);
    }
}
