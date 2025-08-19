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
    }
}
