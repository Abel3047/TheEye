using TheEye.Application.DTOs;
using TheEye.Core.Entities;
using TheEye.Core.Models;

namespace TheEye.Application.Helpers
{
    public class Mapper
    {
        public static EyeSnapshotDto MapEyeStateToSnapShotDto(EyeState s)=> new EyeSnapshotDto()
            {
                Id = s.Id,
                X = s.X,
                Y = s.Y,
                BaseBearing = s.BaseBearing,
                SpeedKmPerDay = s.SpeedKmPerDay,
                DiameterKm = s.DiameterKm,
                DriftVarianceDeg = s.DriftVarianceDeg,
                JitterFraction = s.JitterFraction,
                CourseShiftChancePerDay = s.CourseShiftChancePerDay,
                PredictabilityRating = s.PredictabilityRating,
                Paused = s.Paused,
                LastUpdated = s.LastUpdated,
                TotalElapsedHours = s.TotalElapsedHours,
                ActiveInfluences = s.ActiveInfluences.Select(i => new ActiveInfluenceView
                {
                    Name = i.Name,
                    DirectionDeg = i.DirectionDeg,
                    MagnitudeKmPerDay = i.MagnitudeKmPerDay,
                    RemainingHours = i.RemainingHours
                }).ToList()
            };
        
        public static EyeState MapSnapShotDtotoEyeState(EyeSnapshotDto dto) => new EyeState()
        {
            Id = dto.Id,
            X = dto.X,
            Y = dto.Y,
            BaseBearing = dto.BaseBearing,
            SpeedKmPerDay = dto.SpeedKmPerDay,
            DiameterKm = dto.DiameterKm,
            DriftVarianceDeg = dto.DriftVarianceDeg,
            JitterFraction = dto.JitterFraction,
            CourseShiftChancePerDay = dto.CourseShiftChancePerDay,
            PredictabilityRating = dto.PredictabilityRating,
            Paused = dto.Paused,
            LastUpdated = dto.LastUpdated,
            TotalElapsedHours = dto.TotalElapsedHours,
            ActiveInfluences = dto.ActiveInfluences.Select(i => new ActiveInfluence
            {
                Name = i.Name,
                DirectionDeg = i.DirectionDeg,
                MagnitudeKmPerDay = i.MagnitudeKmPerDay,
                RemainingHours = i.RemainingHours
            }).ToList()
        };

    }
}
