using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Tag : BaseEntity
{
    public string TagName { get; private set; }

    protected Tag() { }

    public Tag(string tagName)
    {
        TagName = tagName;
    }

    public void Update(string tagName)
    {
        TagName = tagName;
    }
}