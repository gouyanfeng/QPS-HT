using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using QPS.Application.Contracts.Auth;
using QPS.Application.Features.Auth;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace QPS.UnitTests;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLoginResponse_WhenValidCredentials()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var jwtGeneratorMock = new Mock<IJwtGenerator>();
        
        var user = User.Create("admin", "123456", "Admin");
        var users = new List<User> { user };
        var usersDbSetMock = CreateDbSetMock(users);
        
        dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        jwtGeneratorMock.Setup(jwt => jwt.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>()))
            .Returns("test-token");
        
        var handler = new LoginHandler(dbContextMock.Object, jwtGeneratorMock.Object);
        var command = new LoginCommand
        {
            Request = new LoginRequest
            {
                Username = "admin",
                Password = "123456"
            }
        };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.RealName, result.RealName);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(user.MerchantId, result.MerchantId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInvalidUsername()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var jwtGeneratorMock = new Mock<IJwtGenerator>();
        
        var users = new List<User>();
        var usersDbSetMock = CreateDbSetMock(users);
        
        dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        
        var handler = new LoginHandler(dbContextMock.Object, jwtGeneratorMock.Object);
        var command = new LoginCommand
        {
            Request = new LoginRequest
            {
                Username = "invalid",
                Password = "123456"
            }
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInvalidPassword()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var jwtGeneratorMock = new Mock<IJwtGenerator>();
        
        var user = User.Create("admin", "123456", "Admin");
        var users = new List<User> { user };
        var usersDbSetMock = CreateDbSetMock(users);
        
        dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        
        var handler = new LoginHandler(dbContextMock.Object, jwtGeneratorMock.Object);
        var command = new LoginCommand
        {
            Request = new LoginRequest
            {
                Username = "admin",
                Password = "invalid"
            }
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIsInactive()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var jwtGeneratorMock = new Mock<IJwtGenerator>();
        
        var user = User.Create("admin", "123456", "Admin");
        user.Deactivate();
        var users = new List<User> { user };
        var usersDbSetMock = CreateDbSetMock(users);
        
        dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        
        var handler = new LoginHandler(dbContextMock.Object, jwtGeneratorMock.Object);
        var command = new LoginCommand
        {
            Request = new LoginRequest
            {
                Username = "admin",
                Password = "123456"
            }
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> entities) where T : class
    {
        var queryable = entities.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        // 为异步查询添加支持
        dbSetMock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        
        return dbSetMock;
    }
    
    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        
        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }
        
        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }
        
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }
        
        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }
        
        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }
        
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider).GetMethods()
                .Where(m => m.Name == "Execute" && m.IsGenericMethod)
                .FirstOrDefault(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Expression));
            
            var executionResult = executeMethod
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new object[] { expression });
            
            var fromResultMethod = typeof(Task).GetMethods()
                .Where(m => m.Name == "FromResult" && m.IsGenericMethod)
                .FirstOrDefault();
            
            return (TResult)fromResultMethod
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult });
        }
    }
    
    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }
        
        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }
        
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
        
        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }
    
    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        
        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }
        
        public T Current
        {
            get { return _inner.Current; }
        }
        
        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
        
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}