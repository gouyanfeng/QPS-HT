namespace QPS.Application.Interfaces;

public interface IJwtGenerator
{
    string GenerateToken(Guid userId, string role);
}
