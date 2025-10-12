using Microsoft.AspNetCore.Mvc;

namespace IntelliAIService;
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