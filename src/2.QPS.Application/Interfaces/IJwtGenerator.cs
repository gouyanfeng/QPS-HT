namespace QPS.Application.Interfaces;

public interface IJwtGenerator
{
    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="merchantId">商户ID</param>
    /// <param name="role">角色</param>
    /// <param name="shopId">店铺ID（可选）</param>
    /// <returns>JWT令牌</returns>
    string GenerateToken(Guid userId, Guid merchantId, string role, Guid? shopId = null);
}