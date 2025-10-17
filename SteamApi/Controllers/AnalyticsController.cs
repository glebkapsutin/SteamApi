using Microsoft.AspNetCore.Mvc;
using SteamApi.Application.Services;

namespace SteamApi.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : ApiControllerBase
    {
        private readonly IApiFacade _facade;

        public AnalyticsController(IApiFacade facade)
        {
            _facade = facade;
        }

        [HttpGet("genres")]
        public async Task<IActionResult> TopGenres([FromQuery] string month, [FromQuery] int top = 5, CancellationToken ct = default)
        {
            var res = await _facade.GetTopGenres(month, top, ct);
            return FromResult(res);
        }

        [HttpGet("dynamics")]
        public async Task<IActionResult> Dynamics(CancellationToken ct)
        {
            var res = await _facade.GetDynamics(ct);
            return FromResult(res);
        }
    }
}


