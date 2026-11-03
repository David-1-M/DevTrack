using Xunit;

namespace DevTrack.Api.Tests;

public class SmokeTests
{
    [Fact]
    public void AdditionTest()
    {
        int result = 5 + 3;

        Assert.Equal(8, result);
    }
}
