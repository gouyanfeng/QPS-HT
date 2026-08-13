using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Infrastructure.Database;

namespace QPS.UnitTests.Common;

internal static class TestDbContextFactory
{
    public static AppDbContext Create(ICurrentUserService? currentUserService = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, currentUserService ?? new TestCurrentUserService());
    }

    public static IPublisher CreatePublisher()
    {
        return new TestPublisher();
    }
}

internal sealed class TestCurrentUserService : ICurrentUserService
{
    public TestCurrentUserService(string? userId = "unit-test", string? username = "unit-test")
    {
        UserId = userId;
        Username = username;
    }

    public string? UserId { get; }

    public string? Username { get; }
}
