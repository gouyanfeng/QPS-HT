using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string ItemName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Amount { get; private set; }

    protected OrderItem() { }

    public OrderItem(Guid orderId, string itemName, decimal unitPrice, int quantity)
    {
        OrderId = orderId;
        ItemName = itemName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Amount = unitPrice * quantity;
    }
}