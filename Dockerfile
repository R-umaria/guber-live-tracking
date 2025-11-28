# ============================
# 1. Base runtime image
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Listen on 8080 inside the container (Portainer stack will map externally)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# ============================
# 2. Build image
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Guber.CoordinatesApi/Guber.CoordinatesApi.csproj", "Guber.CoordinatesApi/"]
RUN dotnet restore "Guber.CoordinatesApi/Guber.CoordinatesApi.csproj"

# Copy the rest of the source and build
COPY . .
WORKDIR "/src/Guber.CoordinatesApi"
RUN dotnet build "Guber.CoordinatesApi.csproj" -c Release -o /app/build

# ============================
# 3. Publish image
# ============================
FROM build AS publish
RUN dotnet publish "Guber.CoordinatesApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================
# 4. Final runtime image
# ============================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Guber.CoordinatesApi.dll"]
