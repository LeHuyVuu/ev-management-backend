var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.ApiGateway>("ApiGateway");
builder.AddProject<Projects.BrandService>("BrandService");
builder.AddProject<Projects.DealerService>("DealerService");

builder.Build().Run();