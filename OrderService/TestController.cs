using Microsoft.AspNetCore.Mvc;

namespace OrderService;
[ApiController]
[Route("[controller]")]
public class TestController
{
    [HttpGet]
    public string Get()
    {
        return "Hello World!";
    }
}