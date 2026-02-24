# Build con contexto = raíz del repo (donde está la carpeta src).
# Desde api_security:  docker build -f Dockerfile -t geonmagno/api_security:0.1.0 ../..
# Desde repo root:     docker build -f src/api_security/Dockerfile -t geonmagno/api_security:0.1.0 .

# Etapa base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5000

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar todos los archivos .csproj (contexto de build = raíz del repo, proyectos en src/)
COPY ["src/api_security/api_security.csproj", "src/api_security/"]
COPY ["src/api_security.application/api_security.application.csproj", "src/api_security.application/"]
COPY ["src/api_security.domain/api_security.domain.csproj", "src/api_security.domain/"]
COPY ["src/api_security.infrastructure/api_security.infrastructure.csproj", "src/api_security.infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "src/api_security/api_security.csproj"

# Copiar todo el código
COPY . .

# Compilar el proyecto principal
WORKDIR "/src/src/api_security"
RUN dotnet build "api_security.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar para despliegue
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api_security.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api_security.dll"]
