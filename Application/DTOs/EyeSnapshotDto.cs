namespace TheEye.Application.DTOs
{
    public class EyeSnapshotDto
    {
        public Guid Id { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double BaseBearing { get; init; }
        public double SpeedKmPerDay { get; init; }
        public double DriftVarianceDeg { get; init; }
        public double JitterFraction { get; init; }
        public double CourseShiftChancePerDay { get; init; }
        public int PredictabilityRating { get; init; }
        public bool Paused { get; init; }
        public DateTime LastUpdated { get; init; }
        public List<ActiveInfluenceView> ActiveInfluences { get; init; } = new();
    }
    public sealed class ActiveInfluenceView
    {
        public string Name { get; init; } = string.Empty;
        public double DirectionDeg { get; init; }
        public double MagnitudeKmPerDay { get; init; }
        public double RemainingHours { get; init; }
    }
}
