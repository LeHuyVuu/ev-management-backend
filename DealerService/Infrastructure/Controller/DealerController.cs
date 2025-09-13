using Microsoft.AspNetCore.Mvc;

namespace ProductService.Infrastructure.Controller;

/// <summary>
/// API chào hỏi.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DealerController : ControllerBase
{
    /// <summary>
    /// Nói xin chào.
    /// </summary>
    /// <param name="name">Tên người cần chào.</param>
    /// <returns>Lời chào.</returns>
    /// <response code="200">Trả về lời chào</response>
    [HttpGet("greet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> Greet([FromQuery] string name = "world")
        => Ok($"Hello, {name}!");
}