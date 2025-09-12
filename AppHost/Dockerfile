FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Nếu có dùng Aspire
RUN dotnet workload list | grep aspire || dotnet workload install aspire

COPY . .    
WORKDIR /src

RUN dotnet restore                   
RUN dotnet publish AppHost.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish . 
ENTRYPOINT ["dotnet", "AppHost.dll"]
