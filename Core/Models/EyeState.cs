namespace TheEye.Core.Models
{
    public sealed class EyeState
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public double X { get; set; } // km
        public double Y { get; set; } // km
        public double BaseBearing { get; set; } // degrees (0 = East, 90 = North)
        public double SpeedKmPerDay { get; set; } // km/day
        public double DriftVarianceDeg { get; set; } // ±deg per day as typical drift
        public double JitterFraction { get; set; } // ± fraction of hourly move for lateral jitter
        public double CourseShiftChancePerDay { get; set; } // e.g., 0.03 for 3% per day
        public int PredictabilityRating { get; set; } = 3; // 1..5
        public bool Paused { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<ActiveInfluence> ActiveInfluences { get; set; } = new();
    }
}
