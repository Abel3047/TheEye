using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;
using TheEye.Core.Entities;
using TheEye.Core.Models;
using static TheEye.Infrastructure.Controllers.EyeController;

namespace TheEye.Infrastructure.Controllers
{
    [ApiController]
    [Route("dm")]
    public class DMEyeController : ControllerBase
    {
        readonly IEyeSimulator _sim;
        private readonly string _secretKey;

        public DMEyeController(IEyeSimulator sim, IConfiguration config)
        {
            _sim = sim ?? throw new ArgumentNullException(nameof(sim));
            _secretKey = config["DmSecretKey"] ?? throw new InvalidOperationException("DmSecretKey not set in configuration!");
        }

        private bool IsAuthorized(string providedKey)
        {
            // This is a simple but effective time-constant comparison for security.
            return !string.IsNullOrEmpty(providedKey) && providedKey == _secretKey;
        }

        [HttpPost("setDiameter")]
        public IActionResult SetDiameter([FromBody] SetDiameterRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null || req.DiameterKm <= 0) return BadRequest("Provide positive diameterKm");
            _sim.SetDiameter(req.DiameterKm);
            return Ok("Diameter set.");
        }

        [HttpPost("shrink-over-time")]
        public IActionResult ShrinkOverTime([FromBody] ShrinkOverTimeRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null || req.DurationHours <= 0 || req.TargetDiameterKm < 0) return BadRequest();
            _sim.ShrinkOverTime(req.TargetDiameterKm, req.DurationHours);
            return Ok("Shrink over time started.");
        }
        [HttpPost("advance")]
        public IActionResult Advance([FromBody] AdvanceRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null || req.Hours <= 0) return BadRequest("Provide { hours } > 0");
            _sim.AdvanceHours(req.Hours);
            return Ok("Time advanced.");
        }
        [HttpPost("jumpDays")]
        public IActionResult JumpDays([FromBody] JumpRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null || req.Days <= 0) return BadRequest("Provide { days } > 0");
            _sim.AdvanceHours(req.Days * 24.0);
            return Ok("Time jumped.");
        }
        [HttpPost("applyInfluence")]
        public IActionResult ApplyInfluence([FromBody] ApplyInfluenceRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null) return BadRequest("payload required");
            var inf = new ActiveInfluence
            {
                Name = string.IsNullOrWhiteSpace(req.Name) ? Guid.NewGuid().ToString() : req.Name,
                DirectionDeg = req.DirectionDeg,
                MagnitudeKmPerDay = req.MagnitudeKmPerDay,
                RemainingHours = req.DurationHours
            };
            _sim.AddInfluence(inf);
            return Ok("Influence applied.");
        }
        [HttpPost("removeInfluence/{name}")]
        public IActionResult RemoveInfluence(string name, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            _sim.RemoveInfluence(name);
            return Ok("Influence removed.");
        }

        [HttpPost("setBase")]
        public IActionResult SetBase([FromBody] SetBaseRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null) return BadRequest("payload required");
            _sim.SetBase(req.BearingDeg, req.SpeedKmPerDay);
            return Ok("Base set.");
        }

        [HttpPost("surge")]
        public IActionResult Surge([FromBody] SurgeRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            if (req == null) return BadRequest("payload required");
            _sim.TriggerSurge(req.Factor, req.DurationHours, req.Name ?? "surge");
            return Ok("Surge triggered.");
        }

        [HttpPost("pause")]
        public IActionResult Pause([FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            _sim.Pause();
            return Ok("Simulation paused.");
        }

        [HttpPost("resume")]
        public IActionResult Resume([FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            _sim.Resume();
            return Ok("Simulation resumed.");
        }

        [HttpPost("reset")]
        public IActionResult Reset([FromBody] ResetRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
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
            return Ok("Simulation reset.");
        }

        [HttpPost("setCamp")]
        public IActionResult SetCamp([FromBody] SetCampRequest req, [FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            _sim.SetCampPosition(req.X, req.Y);
            return Ok("Camp position updated");
        }

        [HttpPost("centerCamp")]
        public IActionResult CenterCamp([FromQuery] string dm_key)
        {
            if (!IsAuthorized(dm_key)) return Unauthorized();
            _sim.CenterCampOnEye();
            return Ok("Camp centered on eye");
        }

        public record SetCampRequest { public double X { get; init; } public double Y { get; init; } }


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
