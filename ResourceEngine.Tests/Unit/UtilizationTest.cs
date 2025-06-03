using ResourceEngine.Usage;
using Xunit;
using static ResourceEngine.Tests.Util.DateTimeExtensions;

namespace ResourceEngine.Tests.Unit;

// Ensures Utilization works correctly
public class UtilizationTest {

    [Fact]
    public void Statistics() {
        var u = new Utilization(1.May(), 2.Days(), [0,1,1,0,0,0,1,0]);
        Assert.Equal(1, u.Max());
        Assert.Equal(3.0/8, u.Average(), 1e-5);
    }
}