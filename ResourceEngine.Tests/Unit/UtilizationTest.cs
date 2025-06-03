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

    [Fact]
    public void Addition() {
        var u1 = new Utilization(1.May(), 2.Days(), [0,1,1,0,0,0,1,0]);
        var u2 = new Utilization(1.May(), 2.Days(), [0,0,1,1,0,0,1,0]);
        var u3 = new Utilization(1.May(), 2.Days(), [0,0,1,1,0,1,1,0]);
        Utilization uSum = u1 + u2 + u3;
        Assert.Equal(3, uSum.Max());
        Assert.Equal(10.0/8, uSum.Average(), 1e-5);
    }

    [Fact]
    public void Fold() {
        var u = new Utilization(1.May(), 2.Days(), [0,1,1,0,0,0,1,0]);
        u.Fold(24.Hours());
        Assert.Equal(2, u.Max());
        Assert.Equal(3.0/8, u.Average(), 1e-5);
    }
}