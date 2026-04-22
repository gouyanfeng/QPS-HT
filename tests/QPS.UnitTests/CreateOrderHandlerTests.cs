using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Features.Orders;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private readonly Mock<IDbContext> _mockDbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMqttService> _mockMqttService;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _mockDbContext = new Mock<IDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockMqttService = new Mock<IMqttService>();
        _handler = new CreateOrderHandler(_mockDbContext.Object, _mockCurrentUserService.Object, _mockMqttService.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenRoomIsAvailable()
    {
        // Arrange
        var tenantId = Guid.Empty;
        var roomId = Guid.NewGuid();
        var shopId = Guid.NewGuid();

        var room = Room.Create(shopId, "测试房间", "DEVICE123", "room/merchant/shop/room1", 30m);

        var mockOrdersDbSet = new Mock<DbSet<Order>>();

        _mockCurrentUserService.Setup(c => c.MerchantId).Returns(tenantId);
        _mockDbContext.Setup(d => d.Rooms.FindAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);
        _mockDbContext.Setup(d => d.Orders).Returns(mockOrdersDbSet.Object);
        _mockMqttService.Setup(m => m.SendCommandAsync(room.MqttTopic, "POWER_ON")).Returns(Task.CompletedTask);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateOrderCommand
        {
            Request = new CreateOrderRequest { RoomId = roomId }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.ToString());
        _mockDbContext.Verify(d => d.Orders.Add(It.IsAny<Order>()), Times.Once);
        _mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMqttService.Verify(m => m.SendCommandAsync(room.MqttTopic, "POWER_ON"), Times.Once);
        Assert.Equal(RoomStatus.Occupied, room.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenRoomDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        _mockCurrentUserService.Setup(c => c.MerchantId).Returns(tenantId);
        _mockDbContext.Setup(d => d.Rooms.FindAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room)null);

        var command = new CreateOrderCommand
        {
            Request = new CreateOrderRequest { RoomId = roomId }
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenRoomIsNotIdle()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var merchantId = tenantId;
        var shopId = Guid.NewGuid();

        var room = Room.Create(shopId, "测试房间", "DEVICE123", "room/merchant/shop/room1", 30m);
        room.Occupy(); // 使房间处于占用状态

        _mockCurrentUserService.Setup(c => c.MerchantId).Returns(tenantId);
        _mockDbContext.Setup(d => d.Rooms.FindAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var command = new CreateOrderCommand
        {
            Request = new CreateOrderRequest { RoomId = roomId }
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenRoomBelongsToDifferentMerchant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var differentMerchantId = Guid.NewGuid();
        var shopId = Guid.NewGuid();

        var room = Room.Create(shopId, "测试房间", "DEVICE123", "room/merchant/shop/room1", 30m);

        _mockCurrentUserService.Setup(c => c.MerchantId).Returns(tenantId);
        _mockDbContext.Setup(d => d.Rooms.FindAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var command = new CreateOrderCommand
        {
            Request = new CreateOrderRequest { RoomId = roomId }
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(command, CancellationToken.None));
    }
}