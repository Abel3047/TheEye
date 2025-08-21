using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;
using TheEye.Core.Entities;
using TheEye.Core.Models;

namespace TheEye.Infrastructure.Controllers
{
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
        public ActionResult<EyeSnapshotDto> Get()
        {
            var state = _sim.GetStateSnapshot();
            return Ok(MapToDto(state));
        }
        [HttpPost("setDiameter")]
        public ActionResult<EyeSnapshotDto> SetDiameter([FromBody] SetDiameterRequest req)
        {
            if (req == null || req.DiameterKm <= 0) return BadRequest("Provide positive diameterKm");
            _sim.SetDiameter(req.DiameterKm);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("shrink-over-time")]
        public IActionResult ShrinkOverTime([FromBody] ShrinkOverTimeRequest req)
        {
            if (req == null || req.DurationHours <= 0 || req.TargetDiameterKm < 0) return BadRequest();
            _sim.ShrinkOverTime(req.TargetDiameterKm, req.DurationHours);
            // return immediate snapshot after scheduling
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("advance")]
        public ActionResult<EyeSnapshotDto> Advance([FromBody] AdvanceRequest req)
        {
            if (req == null || req.Hours <= 0) return BadRequest("Provide { hours } > 0");
            _sim.AdvanceHours(req.Hours);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("jumpDays")]
        public ActionResult<EyeSnapshotDto> JumpDays([FromBody] JumpRequest req)
        {
            if (req == null || req.Days <= 0) return BadRequest("Provide { days } > 0");
            _sim.AdvanceHours(req.Days * 24.0);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("applyInfluence")]
        public ActionResult<EyeSnapshotDto> ApplyInfluence([FromBody] ApplyInfluenceRequest req)
        {
            if (req == null) return BadRequest("payload required");
            var inf = new ActiveInfluence
            {
                Name = string.IsNullOrWhiteSpace(req.Name) ? Guid.NewGuid().ToString() : req.Name,
                DirectionDeg = req.DirectionDeg,
                MagnitudeKmPerDay = req.MagnitudeKmPerDay,
                RemainingHours = req.DurationHours
            };
            _sim.AddInfluence(inf);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("removeInfluence/{name}")]
        public ActionResult<EyeSnapshotDto> RemoveInfluence(string name)
        {
            _sim.RemoveInfluence(name);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("setBase")]
        public ActionResult<EyeSnapshotDto> SetBase([FromBody] SetBaseRequest req)
        {
            if (req == null) return BadRequest("payload required");
            _sim.SetBase(req.BearingDeg, req.SpeedKmPerDay);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("surge")]
        public ActionResult<EyeSnapshotDto> Surge([FromBody] SurgeRequest req)
        {
            if (req == null) return BadRequest("payload required");
            _sim.TriggerSurge(req.Factor, req.DurationHours, req.Name ?? "surge");
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("pause")]
        public ActionResult<EyeSnapshotDto> Pause()
        {
            _sim.Pause();
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("resume")]
        public ActionResult<EyeSnapshotDto> Resume()
        {
            _sim.Resume();
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        [HttpPost("reset")]
        public ActionResult<EyeSnapshotDto> Reset([FromBody] ResetRequest req)
        {
            var s = new EyeState
            {
                X = req?.X ?? 0.0,
                Y = req?.Y ?? 0.0,
                BaseBearing = req?.BaseBearing ?? 90.0,
                SpeedKmPerDay = req?.SpeedKmPerDay ?? 10.0,
                DriftVarianceDeg = req?.DriftVarianceDeg ?? 5.0,
                JitterFraction = req?.JitterFraction ?? 0.08,
                CourseShiftChancePerDay = req?.CourseShiftChancePerDay ?? 0.03,
                PredictabilityRating = req?.PredictabilityRating ?? 3
            };
            _sim.Reset(s);
            return Ok(MapToDto(_sim.GetStateSnapshot()));
        }

        // -------- Helper mapping (Domain -> DTO) --------
        static EyeSnapshotDto MapToDto(EyeState s)
        {
            return new EyeSnapshotDto
            {
                Id = s.Id,
                X = s.X,
                Y = s.Y,
                BaseBearing = s.BaseBearing,
                SpeedKmPerDay = s.SpeedKmPerDay,
                DiameterKm = s.DiameterKm, // NEW
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

        // -------- Request models (kept nearby for convenience) --------
        public record SetDiameterRequest { public double DiameterKm { get; init; } }
        public record ShrinkOverTimeRequest { public double TargetDiameterKm { get; init; } public int DurationHours { get; init; } }
        public record AdvanceRequest { public double Hours { get; init; } }
        public record JumpRequest { public double Days { get; init; } }
        public record ApplyInfluenceRequest
        {
            public string? Name { get; init; }
            public double DirectionDeg { get; init; }
            public double MagnitudeKmPerDay { get; init; }
            public double DurationHours { get; init; }
        }
        public record SetBaseRequest { public double BearingDeg { get; init; } public double SpeedKmPerDay { get; init; } }
        public record SurgeRequest { public string? Name { get; init; } public double Factor { get; init; } public double DurationHours { get; init; } }
        public record ResetRequest
        {
            public double? X { get; init; }
            public double? Y { get; init; }
            public double? BaseBearing { get; init; }
            public double? SpeedKmPerDay { get; init; }
            public double? DriftVarianceDeg { get; init; }
            public double? JitterFraction { get; init; }
            public double? CourseShiftChancePerDay { get; init; }
            public int? PredictabilityRating { get; init; }
        }
    }
}
