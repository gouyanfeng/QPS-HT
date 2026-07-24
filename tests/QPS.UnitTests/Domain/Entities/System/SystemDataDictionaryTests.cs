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
            "system_status",
            "System Status",
            "active",
            "Generic system status",
            1,
            true);

        var newParentId = Guid.NewGuid();

        dictionary.Update("Account Status", "disabled", "Updated description", 2, false, newParentId);

        Assert.Equal("Account Status", dictionary.Name);
        Assert.Equal("disabled", dictionary.Value);
        Assert.Equal("Updated description", dictionary.Description);
        Assert.Equal(2, dictionary.SortOrder);
        Assert.False(dictionary.IsActive);
        Assert.Equal(newParentId, dictionary.ParentId);
    }
}
