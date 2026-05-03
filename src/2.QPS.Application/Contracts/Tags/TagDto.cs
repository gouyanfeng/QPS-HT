namespace QPS.Application.Contracts.Tags;

public class TagDto
{
    public Guid Id { get; set; }
    public string TagName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}