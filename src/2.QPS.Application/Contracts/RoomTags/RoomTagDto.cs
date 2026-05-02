namespace QPS.Application.Contracts.RoomTags;

public class RoomTagDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; }
    public Guid TagId { get; set; }
    public string TagName { get; set; }
}

public class RoomTagRequest
{
    public Guid RoomId { get; set; }
    public Guid TagId { get; set; }
}