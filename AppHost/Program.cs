var builder = DistributedApplication.CreateBuilder(args);

var backendApi = builder.AddProject<Projects.BrandService>("BrandService");
builder.AddProject<Projects.DealerService>("DealerService");
builder.AddProject<Projects.ApiGateway>("ApiGateway");
builder.Build().Run();