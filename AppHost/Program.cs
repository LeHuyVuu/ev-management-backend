var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.ApiGateway>("ApiGateway");
builder.AddProject<Projects.BrandService>("BrandService");
builder.AddProject<Projects.DealerService>("DealerService");
builder.AddProject<Projects.CustomerService>("CustomerService");
builder.AddProject<Projects.IdentityService>("IdentityService");
builder.AddProject<Projects.FinancialService>("FinancialService");
builder.AddProject<Projects.UtilityService>("UtilityService");

builder.Build().Run();