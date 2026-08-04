using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmHerbBases;
using QPS.Domain.Entities.Crm;
using QPS.UnitTests.Common;
using Xunit;

namespace QPS.UnitTests.Features.Crm.CrmHerbBases;

public class CrmHerbBaseCommandTests
{
    [Fact]
    public async Task Create_ShouldNotPersistMainProductAttributes()
    {
        var operatorUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        await using var dbContext = TestDbContextFactory.Create(new TestCurrentUserService(operatorUserId.ToString()));
        var handler = new CreateCrmHerbBaseHandler(dbContext, new TestCurrentUserService(operatorUserId.ToString()));

        var result = await handler.Handle(new CreateCrmHerbBaseCommand
        {
            Request = new CrmHerbBaseCreateRequest
            {
                HerbBaseName = "Multi Product Customer",
                MainProducts = new List<string> { "HUANG_QI", "DANG_GUI" },
                Grade = "A",
                Score = 95,
                Province = "Gansu",
                City = "Dingxi",
                Area = "Longxi",
                Address = "Test address",
                SourcePlatform = "MANUAL",
                SourceId = 3001,
                OwnerUserId = ownerUserId,
                PrimaryContactName = "Primary Contact",
                PrimaryContactPhone = "13900000000",
                Remark = "Created by unit test"
            }
        }, CancellationToken.None);

        var customer = await dbContext.CrmHerbBases.SingleAsync(item => item.BaseName == "Multi Product Customer");
        var subject = await dbContext.CrmHerbBaseSubjects.SingleAsync(item => item.Id == customer.HerbBaseSubjectId);
        var attributes = await dbContext.CrmBusinessEntityAttributes
            .Where(attribute => attribute.EntityId == customer.Id)
            .OrderBy(attribute => attribute.SortOrder)
            .Select(attribute => attribute.AttributeValue)
            .ToListAsync();
        var transferRecord = await dbContext.CrmTransferRecords.SingleAsync();

        Assert.True(result);
        Assert.Empty(attributes);
        Assert.Equal("Primary Contact", subject.PrimaryContactName);
        Assert.Equal("13900000000", subject.PrimaryContactPhone);
        Assert.Equal("CRM_HERB_BASE_SUBJECT", transferRecord.EntityType);
        Assert.Equal(subject.Id, transferRecord.EntityId);
        Assert.Null(transferRecord.FromOwnerUserId);
        Assert.Equal(ownerUserId, transferRecord.ToOwnerUserId);
        Assert.Equal(operatorUserId, transferRecord.OperatorUserId);
        Assert.Equal("Created by unit test", transferRecord.Remark);
    }

    [Fact]
    public async Task GetList_ShouldFilterByBusinessEntityMainProductAttributes()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var createHandler = new CreateCrmHerbBaseHandler(dbContext, new TestCurrentUserService());
        await createHandler.Handle(new CreateCrmHerbBaseCommand
        {
            Request = CreateCustomerRequest("Dang Gui Customer")
        }, CancellationToken.None);
        await createHandler.Handle(new CreateCrmHerbBaseCommand
        {
            Request = CreateCustomerRequest("Tian Ma Customer")
        }, CancellationToken.None);

        var dangGuiCustomer = await dbContext.CrmHerbBases.SingleAsync(item => item.BaseName == "Dang Gui Customer");
        var tianMaCustomer = await dbContext.CrmHerbBases.SingleAsync(item => item.BaseName == "Tian Ma Customer");
        dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute("CRM_HERB_BASE", dangGuiCustomer.Id, "CRM_MAIN_PRODUCT", "DANG_GUI", 1));
        dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute("CRM_HERB_BASE", tianMaCustomer.Id, "CRM_MAIN_PRODUCT", "TIAN_MA", 1));
        await dbContext.SaveChangesAsync();

        var handler = new GetCrmHerbBasesHandler(dbContext);

        var result = await handler.Handle(new GetCrmHerbBasesQuery
        {
            MainProducts = new List<string> { "DANG_GUI" },
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        var customer = Assert.Single(result.List);
        Assert.Equal("Dang Gui Customer", customer.HerbBaseName);
        Assert.Equal(new List<string> { "DANG_GUI" }, customer.MainProducts);
    }

    [Fact]
    public async Task Delete_ShouldHideCustomerFromList()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var customer = CrmHerbBase.Create(
            "Deleted Customer",
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
        dbContext.CrmHerbBases.Add(customer);
        await dbContext.SaveChangesAsync();

        var deleteHandler = new DeleteCrmHerbBaseHandler(dbContext);
        await deleteHandler.Handle(new DeleteCrmHerbBaseCommand { Id = customer.Id }, CancellationToken.None);

        var list = await new GetCrmHerbBasesHandler(dbContext).Handle(new GetCrmHerbBasesQuery
        {
            Keyword = "Deleted Customer"
        }, CancellationToken.None);

        Assert.True(customer.IsDeleted);
        Assert.Empty(list.List);
        Assert.Equal(0, list.TotalCount);
    }

    private static CrmHerbBaseCreateRequest CreateCustomerRequest(string herbBaseName)
    {
        return new CrmHerbBaseCreateRequest
        {
            HerbBaseName = herbBaseName,
            Grade = "B",
            Score = 80,
            Province = "Gansu",
            City = "Dingxi",
            Area = "Longxi",
            Address = "Test address",
            SourcePlatform = "MANUAL",
            Remark = "Created by unit test"
        };
    }
}



