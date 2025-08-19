namespace TheEye.Core.Models
{
    public sealed class ActiveInfluence
    {
        public string Name { get; set; } = "influence";
        public double DirectionDeg { get; set; } // direction of influence (0 = East)
        public double MagnitudeKmPerDay { get; set; } // effectively adds to speed component in that direction
        public double RemainingHours { get; set; } // duration in hours
    }
}
