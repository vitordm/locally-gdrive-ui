using Microsoft.AspNetCore.Mvc;

namespace LocallyGDriveApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DummyController : ControllerBase
{

    [HttpGet]    
    public async Task<IActionResult> Get()
     => Ok(new { msg = "Ok" } );
}
