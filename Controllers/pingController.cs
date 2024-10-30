using Microsoft.AspNetCore.Mvc;

namespace DevCL.Controllers;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase {
    [HttpGet]
    public ActionResult GetPingOk() {
        var data = new { Response = "pong"};
        return Ok(data);
    }
}