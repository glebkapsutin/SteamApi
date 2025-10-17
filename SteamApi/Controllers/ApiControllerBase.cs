using Microsoft.AspNetCore.Mvc;
using SteamApi.Application.Common;

namespace SteamApi.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult FromResult(Result result)
            => result.Success ? Ok() : BadRequest(new { error = result.Error });

        protected IActionResult FromResult<T>(Result<T> result)
            => result.Success ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}


