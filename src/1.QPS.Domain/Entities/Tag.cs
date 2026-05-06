using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Tag : BaseEntity
{
    public string TagName { get; private set; }
    public string Category { get; private set; }

    protected Tag() { }

    public Tag(string tagName, string category = "")
    {
        TagName = tagName;
        Category = category;
    }

    public void Update(string tagName, string category)
    {
        TagName = tagName;
        Category = category;
    }
}