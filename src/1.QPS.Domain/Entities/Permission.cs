using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }

    protected Permission() { }

    public Permission(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public void Update(string name, string code)
    {
        Name = name;
        Code = code;
    }
}