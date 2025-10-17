using Microsoft.AspNetCore.Mvc;
using SteamApi.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace SteamApi.Controllers
{
    [ApiController]
    [Route("api/v1/games")]
    public class GamesController : ApiControllerBase
    {
        private readonly IApiFacade _facade;

        public GamesController(IApiFacade facade)
        {
            _facade = facade;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string month, [FromQuery] string[]? platform, [FromQuery] string[]? tag, CancellationToken ct)
        {
            var res = await _facade.GetGames(month, platform, tag, ct);
            return FromResult(res);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> Calendar([FromQuery] string month, [FromQuery] string[]? platform, [FromQuery] string[]? tag, CancellationToken ct)
        {
            var res = await _facade.GetCalendar(month, platform, tag, ct);
            return FromResult(res);
        }

        [HttpPost("sync")]
        [Authorize]
        public async Task<IActionResult> Sync([FromQuery] string month, CancellationToken ct)
        {
            var res = await _facade.Sync(month, ct);
            return FromResult(res);
        }
    }
}


