using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Review : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Score { get; private set; }
    public string Content { get; private set; }

    protected Review() { }

    public Review(Guid orderId, Guid roomId, Guid customerId, int score, string content)
    {
        OrderId = orderId;
        RoomId = roomId;
        CustomerId = customerId;
        Score = score;
        Content = content;
    }

    public void Update(int score, string content)
    {
        Score = score;
        Content = content;
    }
}