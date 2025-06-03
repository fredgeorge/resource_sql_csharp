namespace ResourceEngine.Usage;

// Understands the occupancy pattern for a Resource
public class Utilization {
    private readonly DateTime _start;
    private readonly TimeSpan _duration;
    private readonly OccupationPattern _pattern;
    private readonly TimeSpan _interval;

    public Utilization(DateTime start, TimeSpan duration, List<int> occupancyCounts) {
        _start = start;
        _duration = duration;
        _interval = _duration/occupancyCounts.Count;
        _pattern = new OccupationPattern(occupancyCounts);
    }

    public int Max() => _pattern.Max();

    public double Average() => _pattern.Average();
    
    private class OccupationPattern(List<int> occupancyCounts) {
        internal int Max() => occupancyCounts.Max();
        internal double Average() => occupancyCounts.Average();
    }
}