using Xunit;
using Moq;
using QPS.Application.Features.Auth;
using QPS.Application.Contracts.Auth;
using QPS.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.UnitTests;

public class LogoutHandlerTests
{
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly LogoutHandler _handler;

    public LogoutHandlerTests()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new LogoutHandler(_mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLogoutResponse_WhenLogoutIsSuccessful()
    {
        // Arrange
        var command = new LogoutCommand
        {
            Request = new LogoutRequest()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("登出成功", result.Message);
    }
}