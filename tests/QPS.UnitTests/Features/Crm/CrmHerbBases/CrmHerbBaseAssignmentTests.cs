using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmHerbBases;
using QPS.Domain.Common;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Entities.System;
using QPS.Domain.Exceptions;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmHerbBases;

public class CrmHerbBaseAssignmentTests
{
    [Fact]
    public async Task AssignOwner_ShouldUpdateCustomerAndCreateTransferRecord_WhenSingleCustomerAssigned()
    {
        var operatorUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(operatorUserId.ToString(), "operator"));
        var operatorUser = AddUser(dbContext, "operator", "操作员真名");
        var fromOwner = AddUser(dbContext, "from-owner", "原负责人");
        var toOwner = AddUser(dbContext, "to-owner", "新负责人");
        var customer = AddCustomer(dbContext, "Single Assign Customer", fromOwner.Id);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(operatorUser, operatorUserId);
        await dbContext.SaveChangesAsync();

        var handler = new AssignCrmHerbBaseOwnerHandler(dbContext, new TestCurrentUserService(operatorUserId.ToString(), null));

        var result = await handler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { customer.Id },
                OwnerUserId = toOwner.Id,
                Remark = "分给销售二组"
            }
        }, CancellationToken.None);

        var updatedCustomer = await dbContext.CrmHerbBases.SingleAsync(c => c.Id == customer.Id);
        var record = await dbContext.CrmTransferRecords.SingleAsync();
        Assert.True(result);
        Assert.Equal(toOwner.Id, updatedCustomer.OwnerUserId);
        Assert.Equal("CRM_HERB_BASE", record.EntityType);
        Assert.Equal(customer.Id, record.EntityId);
        Assert.Equal(fromOwner.Id, record.FromOwnerUserId);
        Assert.Equal(toOwner.Id, record.ToOwnerUserId);
        Assert.Equal(operatorUserId, record.OperatorUserId);
        Assert.Equal("分给销售二组", record.Remark);
    }

    [Fact]
    public async Task AssignOwner_ShouldCreateOneTransferRecordPerCustomer_WhenBatchAssigned()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var owner = AddUser(dbContext, "batch-owner", "批量负责人");
        var first = AddCustomer(dbContext, "Batch Customer 1", null);
        var second = AddCustomer(dbContext, "Batch Customer 2", null);
        await dbContext.SaveChangesAsync();

        var handler = new AssignCrmHerbBaseOwnerHandler(dbContext, new TestCurrentUserService());

        await handler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { first.Id, second.Id },
                OwnerUserId = owner.Id
            }
        }, CancellationToken.None);

        var ownerIds = await dbContext.CrmHerbBases
            .Where(c => c.Id == first.Id || c.Id == second.Id)
            .Select(c => c.OwnerUserId)
            .ToListAsync();
        var recordCount = await dbContext.CrmTransferRecords.CountAsync();

        Assert.All(ownerIds, ownerId => Assert.Equal(owner.Id, ownerId));
        Assert.Equal(2, recordCount);
    }

    [Fact]
    public async Task AssignOwner_ShouldClearOwnerAndRecordEmptyTarget_WhenOwnerUserIdIsNull()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var fromOwner = AddUser(dbContext, "clear-owner", "待清空负责人");
        var customer = AddCustomer(dbContext, "Clear Owner Customer", fromOwner.Id);
        await dbContext.SaveChangesAsync();

        var handler = new AssignCrmHerbBaseOwnerHandler(dbContext, new TestCurrentUserService());

        await handler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { customer.Id },
                OwnerUserId = null,
                Remark = "退回公共池"
            }
        }, CancellationToken.None);

        var updatedCustomer = await dbContext.CrmHerbBases.SingleAsync(c => c.Id == customer.Id);
        var record = await dbContext.CrmTransferRecords.SingleAsync();

        Assert.Null(updatedCustomer.OwnerUserId);
        Assert.Equal(fromOwner.Id, record.FromOwnerUserId);
        Assert.Null(record.ToOwnerUserId);
        Assert.Equal("退回公共池", record.Remark);
    }

    [Fact]
    public async Task AssignOwner_ShouldRollbackAllChanges_WhenAnyCustomerDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var owner = AddUser(dbContext, "rollback-owner", "回滚负责人");
        var customer = AddCustomer(dbContext, "Rollback Customer", null);
        await dbContext.SaveChangesAsync();

        var handler = new AssignCrmHerbBaseOwnerHandler(dbContext, new TestCurrentUserService());

        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { customer.Id, Guid.NewGuid() },
                OwnerUserId = owner.Id
            }
        }, CancellationToken.None));

        var unchangedCustomer = await dbContext.CrmHerbBases.SingleAsync(c => c.Id == customer.Id);
        var recordCount = await dbContext.CrmTransferRecords.CountAsync();

        Assert.Null(unchangedCustomer.OwnerUserId);
        Assert.Equal(0, recordCount);
    }

    [Fact]
    public async Task GetTransferRecords_ShouldReturnNewestRecordsFirst()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var firstOwner = AddUser(dbContext, "first-owner", "第一负责人");
        var secondOwner = AddUser(dbContext, "second-owner", "第二负责人");
        var customer = AddCustomer(dbContext, "Transfer History Customer", null);
        await dbContext.SaveChangesAsync();

        var assignHandler = new AssignCrmHerbBaseOwnerHandler(dbContext, new TestCurrentUserService());
        await assignHandler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { customer.Id },
                OwnerUserId = firstOwner.Id,
                Remark = "第一次"
            }
        }, CancellationToken.None);
        await Task.Delay(5);
        await assignHandler.Handle(new AssignCrmHerbBaseOwnerCommand
        {
            Request = new CrmHerbBaseAssignOwnerRequest
            {
                HerbBaseIds = new List<Guid> { customer.Id },
                OwnerUserId = secondOwner.Id,
                Remark = "第二次"
            }
        }, CancellationToken.None);

        var queryHandler = new GetCrmTransferRecordsHandler(dbContext);

        var records = await queryHandler.Handle(new GetCrmTransferRecordsQuery
        {
            EntityType = "CRM_HERB_BASE",
            EntityId = customer.Id
        }, CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.Equal("第二次", records[0].Remark);
        Assert.Equal(secondOwner.Id, records[0].ToOwnerUserId);
        Assert.Equal("第二负责人", records[0].ToOwnerUserName);
        Assert.Equal("第一次", records[1].Remark);
        Assert.Equal(firstOwner.Id, records[1].ToOwnerUserId);
        Assert.Equal("第一负责人", records[1].ToOwnerUserName);
    }

    private static SystemUser AddUser(DbContext dbContext, string username, string realName)
    {
        var role = new SystemRole($"{username}-role", $"{username}-role");
        var user = SystemUser.Create(username, "hash", realName, role.Id);
        dbContext.Add(role);
        dbContext.Add(user);
        return user;
    }

    private static CrmHerbBase AddCustomer(DbContext dbContext, string herbBaseName, Guid? ownerUserId)
    {
        var customer = CrmHerbBase.Create(
            herbBaseName,
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
            ownerUserId,
            "Remark");
        dbContext.Add(customer);
        return customer;
    }
}




