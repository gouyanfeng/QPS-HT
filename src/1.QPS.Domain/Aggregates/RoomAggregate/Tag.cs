using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.RoomAggregate;

public class Tag : Entity
{
    public Guid MerchantId { get; private set; }
    public string TagName { get; private set; }
    public string Category { get; private set; }

    protected Tag() { }

    public Tag(Guid merchantId, string tagName, string category)
    {
        MerchantId = merchantId;
        TagName = tagName;
        Category = category;
    }

    public void Update(string tagName, string category)
    {
        TagName = tagName;
        Category = category;
    }
}