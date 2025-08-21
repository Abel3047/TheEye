using Microsoft.AspNetCore.Mvc;
using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;
using TheEye.Core.Entities;
using TheEye.Core.Models;

namespace TheEye.Infrastructure.Controllers
{
    // This is the new DTO that holds the complete state for the frontend.
    public class FullStateDto
    {
        public EyeSnapshotDto Eye { get; set; }
        public double CampX { get; set; }
        public double CampY { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class EyeController : ControllerBase
    {
        readonly IEyeSimulator _sim;

        public EyeController(IEyeSimulator sim)
        {
            _sim = sim ?? throw new ArgumentNullException(nameof(sim));
        }

        [HttpGet("")]
        public ActionResult<FullStateDto> Get()
        {
            var eyeState = _sim.GetStateSnapshot();
            var (campX, campY) = _sim.GetCampPosition();

            var fullState = new FullStateDto
            {
                Eye = MapToDto(eyeState), // Use your existing helper
                CampX = campX,
                CampY = campY
            };
            return Ok(fullState);
        }

        // THIS HELPER METHOD STAYS because the Get() method uses it.
        static EyeSnapshotDto MapToDto(EyeState s)
        {
            return new EyeSnapshotDto
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
        }

    }
}