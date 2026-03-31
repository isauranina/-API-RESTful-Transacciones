# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY TransaccionesAPI.sln ./
COPY src/Transacciones.Core/Transacciones.Core.csproj ./src/Transacciones.Core/
COPY src/Transacciones.Infrastructure/Transacciones.Infrastructure.csproj ./src/Transacciones.Infrastructure/
COPY src/Transacciones.API/Transacciones.API.csproj ./src/Transacciones.API/

RUN dotnet restore "./src/Transacciones.API/Transacciones.API.csproj"

COPY src/Transacciones.Core/ ./src/Transacciones.Core/
COPY src/Transacciones.Infrastructure/ ./src/Transacciones.Infrastructure/
COPY src/Transacciones.API/ ./src/Transacciones.API/

RUN dotnet build "./src/Transacciones.API/Transacciones.API.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Transacciones.API/Transacciones.API.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  --no-restore

  # This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
# FORZAR EL PUERTO 5035
ENV ASPNETCORE_URLS=http://+:5035
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transacciones.API.dll"]

# This stage is used in production
#FROM base AS final
#WORKDIR /app
#ENV ASPNETCORE_ENVIRONMENT=Production
#ENV ASPNETCORE_URLS=http://0.0.0.0:5035
#·COPY --from=publish /app/publish .

#ENTRYPOINT ["dotnet", "Transacciones.API.dll"]