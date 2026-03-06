# Build con contexto = raíz del repo. Desde repo root:
# docker build -f src/api_security.WorkerService/Dockerfile -t geonmagno/api_security_worker:0.1.0 .

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/api_security.WorkerService/api_security.WorkerService.csproj", "src/api_security.WorkerService/"]
COPY ["src/api_security.application/api_security.application.csproj", "src/api_security.application/"]
COPY ["src/api_security.domain/api_security.domain.csproj", "src/api_security.domain/"]
COPY ["src/api_security.infrastructure/api_security.infrastructure.csproj", "src/api_security.infrastructure/"]

RUN dotnet restore "src/api_security.WorkerService/api_security.WorkerService.csproj"

COPY . .

WORKDIR "/src/src/api_security.WorkerService"
RUN dotnet build "api_security.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api_security.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api_security.WorkerService.dll"]
