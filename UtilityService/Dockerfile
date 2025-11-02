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

# ---- COPY MKL .so vào output publish (ăn chắc) ----
RUN set -eux; \
    mkdir -p /app/publish/runtimes/linux-x64/native; \
    cp -v /root/.nuget/packages/microsoft.ml.mkl.components/*/runtimes/linux-x64/native/* \
          /app/publish/runtimes/linux-x64/native/ || true; \
    echo "== verify native in publish =="; \
    ls -la /app/publish/runtimes/linux-x64/native

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UtilityService.dll"]
