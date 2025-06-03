using System.ComponentModel;

namespace ResourceEngine.Usage;

// Understands the occupancy pattern for a Resource
public class Utilization {
    private readonly DateTime _start;
    private readonly TimeSpan _duration;
    private readonly OccupationPattern _pattern;
    private readonly TimeSpan _interval;
    private State _state = Initial.Instance;

    public Utilization(DateTime start, TimeSpan duration, List<int> occupancyCounts) {
        _start = start;
        _duration = duration;
        _interval = _duration/occupancyCounts.Count;
        _pattern = new OccupationPattern(occupancyCounts);
    }

    public int Max() => _state.Max(this);

    public double Average() => _state.Average(this);

    private interface State {
        int Max(Utilization u);
        double Average(Utilization u);
    }

    private class Initial : State {
        internal static readonly Initial Instance = new Initial();
        private Initial() {}
        public int Max(Utilization u) => u._pattern.Max();
        public double Average(Utilization u) => u._pattern.Average();
    }
    
    private class OccupationPattern(List<int> occupancyCounts) {
        internal int Max() => occupancyCounts.Max();
        internal double Average() => occupancyCounts.Average();
    }
}