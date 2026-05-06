namespace QPS.Application.Contracts.Rooms;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; }
    public string Status { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; }
    public string ShopAddress { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsEnabled { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public List<RoomImageItemDto> Images { get; set; } = new List<RoomImageItemDto>();
    public List<RoomTagItemDto> Tags { get; set; } = new List<RoomTagItemDto>();
    public List<RoomPlanItemDto> Plans { get; set; } = new List<RoomPlanItemDto>();
}

public class RoomImageItemDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Url { get; set; }
    public int SortOrder { get; set; }
}

public class RoomTagItemDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid TagId { get; set; }
    public string TagName { get; set; }
}

public class RoomPlanItemDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; }
    public decimal Price { get; set; }
}