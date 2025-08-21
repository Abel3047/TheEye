using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;
using TheEye.Core.Entities;
using TheEye.Core.Models;

namespace TheEye.Infrastructure.Services
{
    public class EyeSimulatorService : IEyeSimulator
    {
        // This is now the single source of truth for the simulation state and logic.
        private readonly EyeSimulator _simulator;

        readonly IHistoryRecorder? _recorder;
        // manage any running shrink job
        CancellationTokenSource? _shrinkCts;
        Task? _shrinkTask;

        //DnD real-time to game time session properties
        //default mapping (90 minutes real per simulated day)
        private static readonly double _realSecondsPerSimDay = 90 * 60; // 90 minutes => 5400 seconds
        // inside the shrink task:
        double realMsPerSimHour = (_realSecondsPerSimDay / 24.0) * 1000.0; // 225000 ms for 90 minutes/day

        public EyeSimulatorService(IHistoryRecorder? recorder = null)
        {
            _recorder = recorder;
            var initialState = new EyeState
            {
                X = 0,
                Y = 0,
                BaseBearing = 90.0,
                SpeedKmPerDay = 10.0,
                DiameterKm = 50.0,
                DriftVarianceDeg = 5.0,
                JitterFraction = 0.08,
                CourseShiftChancePerDay = 0.03,
                PredictabilityRating = 3,
                LastUpdated = DateTime.UtcNow // Initialize with current time
            };
            _simulator = new EyeSimulator(initialState);
        }

        static double DegToRad(double deg) => deg * Math.PI / 180.0;
        static double NormalizeDeg(double deg) { var d = deg % 360; if (d < 0) d += 360; return d; }

        // The simulator's internal lock handles thread safety.
        public EyeState GetStateSnapshot()=> _simulator.State;
       
        public void AdvanceHours(double hours)
        {
            _simulator.AdvanceHours(hours);
            // We must update the timestamp whenever time advances.
            lock (_simulator)
            { 
                _simulator.State.LastUpdated = DateTime.UtcNow;
                _simulator.State.TotalElapsedHours += hours;
            }
            _ = RecordSnapshotAsync(); // Optionally record after manual advance
        }
        public void SetDiameter(double diameterKm)
        {
            if (diameterKm < 0) throw new ArgumentOutOfRangeException(nameof(diameterKm));
            // Diameter is not part of the core movement simulation, so we can manage it here.
            lock (_simulator) // Lock on the simulator instance to be safe
            {
                _simulator.State.DiameterKm = diameterKm;
            }
            // We must update the timestamp whenever time advances.
            _simulator.State.LastUpdated = DateTime.UtcNow;
            _ = RecordSnapshotAsync();
        }
        // shrink instantly by percentage (e.g., 20 -> reduces diameter by 20%)
        public void ShrinkByPercent(double percent)
        {
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent));
            lock (_simulator)
            {
                var factor = Math.Max(0.0, 1.0 - percent / 100.0);
                _simulator.State.DiameterKm *= factor;
            }
            // We must update the timestamp whenever time advances.
            _simulator.State.LastUpdated = DateTime.UtcNow;
            _ = RecordSnapshotAsync();
        }       
        public void ShrinkOverTime(double targetDiameterKm, double durationHours)
        {
            if (durationHours <= 0) throw new ArgumentOutOfRangeException(nameof(durationHours));
            if (targetDiameterKm < 0) throw new ArgumentOutOfRangeException(nameof(targetDiameterKm));

            // Lock on the simulator object to safely access its state
            lock (_simulator)
            {
                _shrinkCts?.Cancel();
                _shrinkCts?.Dispose();
                _shrinkCts = new CancellationTokenSource();
                var token = _shrinkCts.Token;

                double start = _simulator.State.DiameterKm;
                double delta = targetDiameterKm - start;
                double perHour = delta / durationHours;

                _shrinkTask = Task.Run(async () =>
                {
                    try
                    {
                        for (int h = 1; h <= durationHours; h++)
                        {
                            if (token.IsCancellationRequested) break;

                            // Advance simulation time by one hour
                            _simulator.AdvanceHours(1.0);

                            // Separately, update the diameter.
                            // Lock here briefly to update the state.
                            lock (_simulator)
                            {
                                _simulator.State.DiameterKm = start + perHour * h;
                                // We must update the timestamp whenever time advances.
                                _simulator.State.LastUpdated = DateTime.UtcNow;
                            }

                            await RecordSnapshotAsync();
                            await Task.Delay((int)realMsPerSimHour, token);
                        }

                        lock (_simulator)
                        {
                            _simulator.State.DiameterKm = targetDiameterKm;
                            // We must update the timestamp whenever time advances.
                            _simulator.State.LastUpdated = DateTime.UtcNow;
                        }
                        await RecordSnapshotAsync();
                    }
                    catch (OperationCanceledException) { /* ignored */ }
                }, token);
            }
        }              
        public void AddInfluence(ActiveInfluence inf) => _simulator.AddInfluence(inf);
        public void RemoveInfluence(string name) => _simulator.RemoveInfluence(name);
        public void SetBase(double bearingDeg, double speedKmPerDay)
        {
            _simulator.SetBase(bearingDeg, speedKmPerDay);
            // We must update the timestamp whenever time advances.
            _simulator.State.LastUpdated = DateTime.UtcNow;
            _ = RecordSnapshotAsync();
        }
        public void TriggerSurge(double factor, double durationHours, string surgeName = "surge")=>
             _simulator.TriggerSurge(factor, durationHours, surgeName);
        public void Pause() => _simulator.Pause();
        public void Resume() => _simulator.Resume();
        public void Reset(EyeState newState)
        {
            newState.LastUpdated = DateTime.UtcNow;
            newState.TotalElapsedHours = 0;
            _simulator.Reset(newState);
            _ = RecordSnapshotAsync();
        }

        async Task RecordSnapshotAsync()
        {
            if (_recorder == null) return;
            try
            {
                var state = GetStateSnapshot(); // Get the current state safely
                var dto = new EyeSnapshotDto
                {
                    Id = state.Id,
                    X = state.X,
                    Y = state.Y,
                    BaseBearing = state.BaseBearing,
                    SpeedKmPerDay = state.SpeedKmPerDay,
                    DiameterKm = state.DiameterKm,
                    DriftVarianceDeg = state.DriftVarianceDeg,
                    JitterFraction = state.JitterFraction,
                    CourseShiftChancePerDay = state.CourseShiftChancePerDay,
                    PredictabilityRating = state.PredictabilityRating,
                    Paused = state.Paused,
                    LastUpdated = state.LastUpdated,
                    ActiveInfluences = state.ActiveInfluences.Select(i => new ActiveInfluenceView
                    {
                        Name = i.Name,
                        DirectionDeg = i.DirectionDeg,
                        MagnitudeKmPerDay = i.MagnitudeKmPerDay,
                        RemainingHours = i.RemainingHours
                    }).ToList()
                };
                await _recorder.RecordAsync(dto);
            }
            catch { /* swallow */ }
        }
    

    }
}
