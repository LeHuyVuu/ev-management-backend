using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller;

/// <summary>
/// API chào hỏi.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
 
    [HttpGet("greet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> Greet()
        => Ok($"Hello, BRAND SERVICE!");
}