using TheEye.Application.Interfaces;
using TheEye.Core.Models;

namespace TheEye.Infrastructure.Services
{
    public class EyeSimulatorService : IEyeSimulator
    {
        readonly object _lock = new();
        readonly Random _rng = new();
        public EyeState State { get; private set; }

        public EyeSimulatorService()
        {
            State = new EyeState
            {
                X = 0,
                Y = 0,
                BaseBearing = 90.0,
                SpeedKmPerDay = 10.0,
                DiameterKm = 50.0, // default
                DriftVarianceDeg = 5.0,
                JitterFraction = 0.08,
                CourseShiftChancePerDay = 0.03,
                PredictabilityRating = 3
            };
        }

        static double DegToRad(double deg) => deg * Math.PI / 180.0;
        static double NormalizeDeg(double deg) { var d = deg % 360; if (d < 0) d += 360; return d; }

        public EyeState GetStateSnapshot()
        {
            lock (_lock)
            {
                // Return the state reference - the API should map to DTO so it doesn't expose domain internals
                return State;
            }
        }

        public void AdvanceHours(double hours)
        {
            if (hours <= 0) return;
            lock (_lock)
            {
                if (State.Paused) { State.LastUpdated = DateTime.UtcNow; return; }

                double remaining = hours;
                while (remaining > 1e-6)
                {
                    double step = Math.Min(1.0, remaining);
                    StepHour(step);
                    remaining -= step;
                }
                State.LastUpdated = DateTime.UtcNow;
            }
        }

        void StepHour(double hours)
        {
            double hourFraction = hours / 24.0;
            double baseSpeed = State.SpeedKmPerDay;
            double moveDistance = baseSpeed * hourFraction;
            double hourlyDriftDeg = State.DriftVarianceDeg * hourFraction;
            double drift = (_rng.NextDouble() * 2.0 - 1.0) * hourlyDriftDeg;

            double influenceX = 0.0, influenceY = 0.0;
            if (State.ActiveInfluences.Count > 0)
            {
                foreach (var inf in State.ActiveInfluences.ToList())
                {
                    double magThisHour = inf.MagnitudeKmPerDay * hourFraction;
                    double rad = DegToRad(inf.DirectionDeg);
                    influenceX += magThisHour * Math.Cos(rad);
                    influenceY += magThisHour * Math.Sin(rad);
                    inf.RemainingHours -= hours;
                }
                State.ActiveInfluences.RemoveAll(i => i.RemainingHours <= 0);
            }

            double baseRad = DegToRad(State.BaseBearing + drift);
            double baseX = moveDistance * Math.Cos(baseRad);
            double baseY = moveDistance * Math.Sin(baseRad);

            double finalX = baseX + influenceX;
            double finalY = baseY + influenceY;

            double lateralJitter = moveDistance * State.JitterFraction * (_rng.NextDouble() * 2 - 1);
            double perpRad = baseRad + Math.PI / 2.0;
            finalX += lateralJitter * Math.Cos(perpRad);
            finalY += lateralJitter * Math.Sin(perpRad);

            State.X += finalX;
            State.Y += finalY;

            double hourlyProb = 1.0 - Math.Pow(1.0 - State.CourseShiftChancePerDay, hours / 24.0);
            if (_rng.NextDouble() < hourlyProb)
            {
                double shift = (_rng.NextDouble() * 60.0) - 30.0;
                State.BaseBearing = NormalizeDeg(State.BaseBearing + shift);
            }

            double slowWanderDeg = (_rng.NextDouble() * 2 - 1) * (State.DriftVarianceDeg * 0.02 * hourFraction * State.PredictabilityRating);
            State.BaseBearing = NormalizeDeg(State.BaseBearing + slowWanderDeg);
        }

        public void SetDiameter(double diameterKm)
        {
            if (diameterKm < 0) throw new ArgumentOutOfRangeException(nameof(diameterKm));
            lock (_lock)
            {
                State.DiameterKm = diameterKm;
            }
        }

        // shrink instantly by percentage (e.g., 20 -> reduces diameter by 20%)
        public void ShrinkByPercent(double percent)
        {
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent));
            lock (_lock)
            {
                var factor = Math.Max(0.0, 1.0 - percent / 100.0);
                State.DiameterKm = State.DiameterKm * factor;
            }
        }


        public void AddInfluence(ActiveInfluence inf)
        {
            lock (_lock) { State.ActiveInfluences.Add(inf); }
        }

        public void RemoveInfluence(string name)
        {
            lock (_lock) { State.ActiveInfluences.RemoveAll(i => i.Name == name); }
        }

        public void SetBase(double bearingDeg, double speedKmPerDay)
        {
            lock (_lock)
            {
                State.BaseBearing = NormalizeDeg(bearingDeg);
                State.SpeedKmPerDay = speedKmPerDay;
            }
        }

        public void TriggerSurge(double factor, double durationHours, string surgeName = "surge")
        {
            var inf = new ActiveInfluence
            {
                Name = surgeName,
                DirectionDeg = State.BaseBearing,
                MagnitudeKmPerDay = State.SpeedKmPerDay * (factor - 1.0),
                RemainingHours = durationHours
            };
            AddInfluence(inf);
        }

        public void Pause() { lock (_lock) State.Paused = true; }
        public void Resume() { lock (_lock) State.Paused = false; }

        public void Reset(EyeState newState)
        {
            lock (_lock)
            {
                State = newState ?? throw new ArgumentNullException(nameof(newState));
            }
        }
    }
}
