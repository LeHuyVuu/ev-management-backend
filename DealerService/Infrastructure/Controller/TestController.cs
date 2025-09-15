using Microsoft.AspNetCore.Mvc;

namespace ProductService.Infrastructure.Controller;

/// <summary>
/// API test gửi message Kafka
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test hello từ ProductService
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> Greet()
        => Ok("Hello from PRODUCT SERVICE!");


}