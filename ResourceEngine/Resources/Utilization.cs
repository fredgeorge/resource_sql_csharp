namespace ResourceEngine.Usage;

// Understands the occupancy pattern for a Resource
public class Utilization {
    private readonly DateTime _start;
    private readonly TimeSpan _duration;
    private readonly OccupationPattern _pattern;
    private readonly TimeSpan _interval;
    private State _state = Initial.Instance;

    public Utilization(DateTime start, TimeSpan duration, List<int> occupancyCounts)
        : this(start, duration, new OccupationPattern(occupancyCounts)) { }

    private Utilization(DateTime start, TimeSpan duration, OccupationPattern pattern) {
        _start = start;
        _duration = duration;
        _interval = _duration / pattern.Count;
        _pattern = pattern;
    }

    public int Max() => _state.Max(this);

    public double Average() => _state.Average(this);

    public static Utilization operator +(Utilization left, Utilization right) => new(
        new[] { left._start, right._start }.Min(),
        left._duration,
        left._pattern + right._pattern);

    public Utilization Fold(TimeSpan duration) {
        _state = new Folded(this, duration);
        return this;
    }

    private interface State {
        int Max(Utilization u);
        double Average(Utilization u);
    }

    private class Initial : State {
        internal static readonly Initial Instance = new Initial();
        private Initial() { }
        public int Max(Utilization u) => u._pattern.Max();
        public double Average(Utilization u) => u._pattern.Average();
    }

    private class Folded : State {
        private readonly Utilization _utilization;
        private readonly int _foldedCount;
        private readonly OccupationPattern _foldedPattern;
        public Folded(Utilization utilization, TimeSpan duration) {
            _utilization = utilization;
            _foldedCount = (int)Math.Ceiling(duration / utilization._interval);
            _foldedPattern = utilization._pattern.Fold(_foldedCount);
        }

        public int Max(Utilization u) => _foldedPattern.Max();

        public double Average(Utilization u) => _foldedPattern.Average();
    }

    private class OccupationPattern {
        private readonly List<int> _occupancyCounts;

        internal OccupationPattern(List<int> occupancyCounts) => 
            _occupancyCounts = occupancyCounts;

        internal int Count => _occupancyCounts.Count;

        internal int Max() => _occupancyCounts.Max();

        internal double Average() => _occupancyCounts.Average();

        public static OccupationPattern operator +(
            OccupationPattern left,
            OccupationPattern right
        ) => new(left._occupancyCounts
            .Zip(right._occupancyCounts)
            .Select(tuple => tuple.First + tuple.Second)
            .ToList());

        internal OccupationPattern Fold(int count) => 
            Split(count).Aggregate((result, counts) => result + counts);

        private IEnumerable<OccupationPattern> Split(int n) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
            var size = (int)Math.Ceiling(_occupancyCounts.Count / (double)n);
            // if (source.Count % n != 0)
            //     throw new ArgumentException("List length must be divisible by n.");

            for (var i = 0; i < _occupancyCounts.Count; i += size)
                yield return new OccupationPattern(_occupancyCounts.Skip(i).Take(size).ToList());
        }
    }
}