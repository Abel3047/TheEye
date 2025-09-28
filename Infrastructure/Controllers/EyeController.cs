using Microsoft.AspNetCore.Mvc;
using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;
using TheEye.Application.Helpers;

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
                Eye = Mapper.MapEyeStateToSnapShotDto(eyeState),
                CampX = campX,
                CampY = campY
            };
            return Ok(fullState);
        }

    }
}