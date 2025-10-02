# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 18888   # Aspire dashboard (nếu muốn truy cập)

# Build with Aspire SDK
FROM mcr.microsoft.com/dotnet/aspire-build:8.0 AS build
WORKDIR /src
COPY AppHost.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish AppHost.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppHost.dll"]
