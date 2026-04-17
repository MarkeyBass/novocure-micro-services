# Stage 1 — build & publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so this layer is cached unless the project file changes.
COPY HousingApi.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — runtime image (~220 MB vs ~900 MB for the SDK image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Listen on HTTP port 8080 inside the container.
# The Mongo connection string is injected at runtime via HousingDatabase__ConnectionString.
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "HousingApi.dll"]
