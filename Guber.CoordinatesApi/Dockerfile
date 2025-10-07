# ==========================
# Guber.CoordinatesApi Dockerfile
# ==========================

# 1️ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy project files
COPY . .

# restore and publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 2️ Runtime stage (lighter image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# copy published output from build stage
COPY --from=build /app/publish .

# expose port (inside container)
EXPOSE 8080

# run the API
ENTRYPOINT ["dotnet", "Guber.CoordinatesApi.dll"]
