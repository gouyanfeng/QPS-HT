using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBases;

internal static class CrmHerbBaseMainProducts
{
    public const string CustomerEntityType = "CRM_HERB_BASE";
    public const string MainProductAttributeCode = "CRM_MAIN_PRODUCT";

    public static List<string> Normalize(IEnumerable<string>? mainProducts, string? mainProduct)
    {
        var values = new List<string>();

        if (mainProducts is not null)
        {
            foreach (var item in mainProducts)
            {
                values.AddRange(SplitValues(item));
            }
        }

        if (!values.Any() && !string.IsNullOrWhiteSpace(mainProduct))
        {
            values.AddRange(SplitValues(mainProduct));
        }

        return values
            .Select(NormalizeValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string BuildSummary(IReadOnlyCollection<string> mainProducts)
    {
        return string.Join(",", mainProducts);
    }

    public static void Sync(IDbContext dbContext, Guid herbBaseId, IReadOnlyCollection<string> mainProducts)
    {
        var existingAttributes = dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == CustomerEntityType &&
                attribute.EntityId == herbBaseId &&
                attribute.AttributeCode == MainProductAttributeCode)
            .ToList();

        dbContext.CrmBusinessEntityAttributes.RemoveRange(existingAttributes);

        var sortOrder = 1;
        foreach (var mainProduct in mainProducts)
        {
            dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute(
                CustomerEntityType,
                herbBaseId,
                MainProductAttributeCode,
                mainProduct,
                sortOrder++));
        }
    }

    public static async Task FillAsync(IDbContext dbContext, List<CrmHerbBaseDto> customers, CancellationToken cancellationToken)
    {
        var herbBaseIds = customers.Select(customer => customer.Id).ToList();
        if (herbBaseIds.Count == 0)
        {
            return;
        }

        var attributes = await dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CustomerEntityType &&
                attribute.AttributeCode == MainProductAttributeCode &&
                herbBaseIds.Contains(attribute.EntityId))
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .ToListAsync(cancellationToken);

        var attributeLookup = attributes
            .GroupBy(attribute => attribute.EntityId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(attribute => attribute.AttributeValue).ToList());

        foreach (var customer in customers)
        {
            if (attributeLookup.TryGetValue(customer.Id, out var mainProducts) && mainProducts.Count > 0)
            {
                customer.MainProducts = mainProducts;
                customer.MainProduct = BuildSummary(mainProducts);
                continue;
            }

            customer.MainProducts = Normalize(null, customer.MainProduct);
        }
    }

    private static string NormalizeValue(string value)
    {
        var trimmedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return string.Empty;
        }

        return trimmedValue switch
        {
            "黄芪" => "HUANG_QI",
            "黃芪" => "HUANG_QI",
            "当归" => "DANG_GUI",
            "當歸" => "DANG_GUI",
            "党参" => "DANG_SHEN",
            "黨參" => "DANG_SHEN",
            "天麻" => "TIAN_MA",
            "多品类" => "OTHER",
            "多品類" => "OTHER",
            "其他" => "OTHER",
            _ => trimmedValue.ToUpperInvariant()
        };
    }

    private static string[] SplitValues(string value)
    {
        return value.Split(',', '，', ';', '；', '/', '、');
    }
}



