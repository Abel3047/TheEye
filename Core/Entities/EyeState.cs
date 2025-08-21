using TheEye.Core.Models;

namespace TheEye.Core.Entities
{
    public sealed class EyeState
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public double X { get; set; } // km
        public double Y { get; set; } // km

        // Direction & movement
        public double BaseBearing { get; set; } // degrees (0 = East, 90 = North)
        public double SpeedKmPerDay { get; set; } // km/day
        public double TotalElapsedHours { get; set; }

        // Diameter of the Eye (km)
        public double DiameterKm { get; set; } = 50.0;

        // drift / jitter / predictability
        public double DriftVarianceDeg { get; set; } // ±deg per day
        public double JitterFraction { get; set; } // fraction of move for lateral jitter
        public double CourseShiftChancePerDay { get; set; } // e.g., 0.03 for 3%/day
        public int PredictabilityRating { get; set; } = 3;
        public bool Paused { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<ActiveInfluence> ActiveInfluences { get; } = new();

    }
}
