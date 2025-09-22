using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc;


namespace BrandService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    
    private readonly string _bootstrapServers;

    public TestController(IConfiguration config)
    {
        _bootstrapServers = config["Kafka:BootstrapServers"] ?? throw new Exception("Kafka config missing");
    }
 
    [HttpGet("v1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> Greet()
        => Ok($"Hello, BRAND SERVICE!");
    
    
    

    
    [HttpGet("test-connection")]
    public IActionResult TestKafkaConnection()
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = _bootstrapServers,
            SocketTimeoutMs = 5000,
            ApiVersionRequestTimeoutMs = 5000
        };

        try
        {
            using var adminClient = new AdminClientBuilder(config).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            var result = new
            {
                Status = "Connected",
                Brokers = metadata.Brokers.Select(b => $"{b.Host}:{b.Port}"),
                Topics = metadata.Topics.Select(t => new
                {
                    t.Topic,
                    PartitionCount = t.Partitions.Count,
                    t.Error
                })
            };

            return Ok(result);
        }
        catch (KafkaException kex)
        {
            return StatusCode(500, new
            {
                Status = "KafkaException",
                Error = kex.Error.Reason,
                IsFatal = kex.Error.IsFatal,
                Code = kex.Error.Code.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Connection Failed",
                Error = ex.Message,
                InnerException = ex.InnerException?.Message
            });
        }
    }
    
    

    public class CreateTopicRequest
    {
        public string TopicName { get; set; } = string.Empty;
        public int NumPartitions { get; set; } = 1;
        public short ReplicationFactor { get; set; } = 1;
    }

    [HttpPost("create-topic")]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TopicName))
            return BadRequest(new { error = "TopicName is required" });

        var config = new AdminClientConfig
        {
            BootstrapServers = _bootstrapServers
        };

        try
        {
            using var adminClient = new AdminClientBuilder(config).Build();

            var topicSpec = new TopicSpecification()
            {
                Name = request.TopicName,
                NumPartitions = request.NumPartitions,
                ReplicationFactor = request.ReplicationFactor
            };

            await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });

            return Ok(new
            {
                status = "success",
                message = $"Topic '{request.TopicName}' created successfully."
            });
        }
        catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            return Conflict(new
            {
                status = "exists",
                message = $"Topic '{request.TopicName}' already exists."
            });
        }
        catch (CreateTopicsException ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Results[0].Error.Reason
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
    
  
}

