using QPS.Domain.Entities.System;
using Xunit;

namespace QPS.UnitTests.Domain.Entities.System;

public class SystemDataDictionaryTests
{
    [Fact]
    public void Update_ShouldUpdateParentId_WhenProvided()
    {
        var dictionary = new SystemDataDictionary(
            Guid.NewGuid(),
            "room_status",
            "房间状态",
            "Idle",
            "房间状态枚举",
            1,
            true);

        var newParentId = Guid.NewGuid();

        dictionary.Update("房间状态新名称", "Occupied", "更新后的描述", 2, false, newParentId);

        Assert.Equal("房间状态新名称", dictionary.Name);
        Assert.Equal("Occupied", dictionary.Value);
        Assert.Equal("更新后的描述", dictionary.Description);
        Assert.Equal(2, dictionary.SortOrder);
        Assert.False(dictionary.IsActive);
        Assert.Equal(newParentId, dictionary.ParentId);
    }
}
