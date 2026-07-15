using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemRole : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }

    public SystemRole(string name, string code)
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