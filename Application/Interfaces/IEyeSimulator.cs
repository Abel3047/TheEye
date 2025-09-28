using TheEye.Core.Entities;
using TheEye.Core.Models;

namespace TheEye.Application.Interfaces
{
    public interface IEyeSimulator
    {
        EyeState GetStateSnapshot();
        void AdvanceHours(double hours);
        void AddInfluence(ActiveInfluence inf);
        void RemoveInfluence(string name);
        void SetBase(double bearingDeg, double speedKmPerDay);
        void TriggerSurge(double factor, double durationHours, string surgeName = "surge");
        void Pause();
        void Resume();
        void Reset(EyeState newState);
        void SetDiameter(double diameterKm);
        /// <summary>
        /// Percent cannot be more than 100 or less than 0.
        /// </summary>
        /// <param name="percent"></param>
        void ShrinkByPercent(double percent);
        /// <summary>
        /// Shrink the Eye diameter from current value to targetDiameter (km) evenly over durationHours simulated hours.
        /// The simulator will advance internal simulation by each hour step and record snapshots via IHistoryRecorder.
        /// This call is non-blocking; it schedules the shrink operation.
        /// </summary>
        void ShrinkOverTime(double targetDiameterKm, double durationHours);

        (double x, double y) GetCampPosition();
        void SetCampPosition(double x, double y);
        void CenterCampOnEye();
        string ExportEye();
        void ImportEye(string eyeSnapshot);
    }
}
