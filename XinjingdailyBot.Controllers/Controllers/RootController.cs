using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XinjingdailyBot.Controllers.Responses;
using XinjingdailyBot.Infrastructure;

namespace SteamLevelupCsharp.Controllers.Controllers;

[ApiController]
public class RootController(IOptions<OptionsSetting> _options) : ControllerBase
{
    [HttpGet("/")]
    public IActionResult GetIndex()
    {
        return _options.Value.Swagger ? Redirect("/swagger") : Redirect("/about");
    }

    [HttpGet("/about")]
    public ActionResult<AboutResponse> GetAbout()
    {
        return Ok(new AboutResponse());
    }
}
