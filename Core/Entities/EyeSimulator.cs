using TheEye.Core.Models;

namespace TheEye.Core.Entities
{
    class EyeSimulator
    {
        readonly object _lock = new();
        readonly Random _rng = new();
        public EyeState State { get; private set; }

        public EyeSimulator(EyeState initial)
        {
            State = initial;
        }

        // Convert degrees to radians
        static double DegToRad(double deg) => deg * Math.PI / 180.0;

        // Normalize bearing to [0,360)
        static double NormalizeDeg(double deg)
        {
            var d = deg % 360;
            if (d < 0) d += 360;
            return d;
        }

        // Add an influence (weight)
        public void AddInfluence(ActiveInfluence inf)
        {
            lock (_lock)
            {
                State.ActiveInfluences.Add(inf);
            }
        }

        public void RemoveInfluence(string name)
        {
            lock (_lock)
            {
                State.ActiveInfluences.RemoveAll(i => i.Name == name);
            }
        }

        // Directly set base parameters
        public void SetBase(double bearingDeg, double speedKmPerDay)
        {
            lock (_lock)
            {
                State.BaseBearing = NormalizeDeg(bearingDeg);
                State.SpeedKmPerDay = speedKmPerDay;
            }
        }

        // Trigger a surge: multiply speed for durationHours
        public void TriggerSurge(double factor, double durationHours, string surgeName = "surge")
        {
            var inf = new ActiveInfluence
            {
                Name = surgeName,
                DirectionDeg = State.BaseBearing, // surge generally along base bearing but can be parameterized
                MagnitudeKmPerDay = State.SpeedKmPerDay * (factor - 1.0), // extra speed
                RemainingHours = durationHours
            };
            AddInfluence(inf);
        }

        // Advance the simulation by hours (can be fractional)
        public void AdvanceHours(double hours)
        {
            if (hours <= 0) return;
            lock (_lock)
            {
                if (State.Paused) { State.LastUpdated = DateTime.UtcNow; return; }

                // We'll step by the lesser of 1 hour or small fractional chunks for stability
                double remaining = hours;
                while (remaining > 1e-6)
                {
                    double step = Math.Min(1.0, remaining); // 1 hour steps
                    StepHour(step);
                    remaining -= step;
                }

                State.LastUpdated = DateTime.UtcNow;
            }
        }

        // Implementation of a single hourly tick
        void StepHour(double hours)
        {
            // Convert daily parameters into hourly-scaled versions
            double hourFraction = hours / 24.0;
            double baseSpeed = State.SpeedKmPerDay; // km/day nominal
            double moveDistance = baseSpeed * hourFraction; // km to move this tick

            // Drift variance scaled to hour: assume DriftVarianceDeg is daily ±; hourly scaled linearly
            double hourlyDriftDeg = State.DriftVarianceDeg * hourFraction;

            // Compute random drift for this tick
            double drift = (_rng.NextDouble() * 2.0 - 1.0) * hourlyDriftDeg;

            // Compute active influences vector sum (convert magnitudes to km/day then km this hour)
            double influenceX = 0.0;
            double influenceY = 0.0;
            if (State.ActiveInfluences.Count > 0)
            {
                foreach (var inf in State.ActiveInfluences)
                {
                    // inf.MagnitudeKmPerDay is additional km/day in inf.DirectionDeg
                    double magThisHour = inf.MagnitudeKmPerDay * hourFraction;
                    double rad = DegToRad(inf.DirectionDeg);
                    influenceX += magThisHour * Math.Cos(rad);
                    influenceY += magThisHour * Math.Sin(rad);

                    // decay
                    inf.RemainingHours -= hours;
                }
                // remove expired
                State.ActiveInfluences.RemoveAll(i => i.RemainingHours <= 0);
            }

            // Also compute weighting from combined influences: they modify final heading
            // We'll calculate a base heading vector from base bearing & base speed
            double baseRad = DegToRad(State.BaseBearing + drift);
            double baseX = moveDistance * Math.Cos(baseRad);
            double baseY = moveDistance * Math.Sin(baseRad);

            // Influence is already in km this hour (influenceX/Y)
            double finalX = baseX + influenceX;
            double finalY = baseY + influenceY;

            // Jitter lateral offset magnitude
            double lateralJitter = moveDistance * State.JitterFraction * (_rng.NextDouble() * 2 - 1);
            // compute a perpendicular to the base heading for lateral jitter
            double perpRad = baseRad + Math.PI / 2.0;
            finalX += lateralJitter * Math.Cos(perpRad);
            finalY += lateralJitter * Math.Sin(perpRad);

            // Apply movement to position
            State.X += finalX;
            State.Y += finalY;

            // Now apply small probabilistic course shift: convert daily chance to hourly probability
            double hourlyProb = 1.0 - Math.Pow(1.0 - State.CourseShiftChancePerDay, hours / 24.0);
            if (_rng.NextDouble() < hourlyProb)
            {
                // course shift occurs: pick a new base bearing within ±30° randomly
                double shift = (_rng.NextDouble() * 60.0) - 30.0;
                State.BaseBearing = NormalizeDeg(State.BaseBearing + shift);
            }

            // Additionally apply small wandering jitter to base bearing (slow wander)
            double slowWanderDeg = (_rng.NextDouble() * 2 - 1) * (State.DriftVarianceDeg * 0.02 * hourFraction * State.PredictabilityRating);
            State.BaseBearing = NormalizeDeg(State.BaseBearing + slowWanderDeg);
        }

        public void Pause() { lock (_lock) State.Paused = true; }
        public void Resume() { lock (_lock) State.Paused = false; }
        public void Reset(EyeState s)
        {
            lock (_lock) { State = s; }
        }

    }    
   
}

