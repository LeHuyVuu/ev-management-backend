FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y libomp5 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY UtilityService.csproj .
RUN dotnet nuget locals all --clear
RUN dotnet restore --runtime linux-x64
COPY . .
RUN dotnet publish UtilityService.csproj -c Release -r linux-x64 --self-contained false \
    -o /app/publish /p:UseAppHost=false /p:PublishTrimmed=false

RUN set -eux; \
    mkdir -p /app/publish/runtimes/linux-x64/native; \
    find /root/.nuget/packages/microsoft.ml.mkl.components -type f \( -name "MklImports.so" -o -name "libiomp5.so" \) \
      -exec cp -v {} /app/publish/runtimes/linux-x64/native/ \; || true; \
    echo "=== Verify native copied ==="; \
    ls -la /app/publish/runtimes/linux-x64/native || true

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UtilityService.dll"]
