using FluentValidation;
using QPS.Application.Contracts.Rooms;

namespace QPS.Application.Validators.Rooms;

/// <summary>
/// 房间创建请求验证器
/// </summary>
public class RoomCreateRequestValidator : AbstractValidator<RoomCreateRequest>
{
    /// <summary>
    /// 构造函数，配置验证规则
    /// </summary>
    public RoomCreateRequestValidator()
    {
        // 验证房间号
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("房间号不能为空")
            .MaximumLength(50).WithMessage("房间号长度不能超过50个字符");

        // 验证店铺ID
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("店铺ID不能为空");

        // 验证单价
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("单价必须大于0");

        // 验证是否启用
        RuleFor(x => x.IsEnabled)
            .NotNull().WithMessage("是否启用不能为空");
    }
}