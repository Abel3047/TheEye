namespace TheEye.Core.Models
{
    public record EyeSnapshot
    {
    public Guid Id { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double BaseBearing { get; init; }
    public double DiameterKm { get; set; }
    public double SpeedKmPerDay { get; init; }
    public double DriftVarianceDeg { get; init; }
    public double JitterFraction { get; init; }
    public double CourseShiftChancePerDay { get; init; }
    public int PredictabilityRating { get; init; }
    public bool Paused { get; init; }
    public List<ActiveInfluence> ActiveInfluences { get; init; } = new();
    public DateTime LastUpdated { get; init; }
}
}
