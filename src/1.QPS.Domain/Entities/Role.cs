using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Role : BaseEntity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }

    protected Role() { }

    public Role(Guid merchantId, string name, string code)
    {
        MerchantId = merchantId;
        Name = name;
        Code = code;
    }

    public void Update(string name, string code)
    {
        Name = name;
        Code = code;
    }
}