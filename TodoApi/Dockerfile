# Stage 1 — build & publish
# Use the full SDK image so we have dotnet restore + publish available.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies first.
# Docker caches this layer separately — if only source files change, restore is skipped.
COPY TodoApi.csproj ./
RUN dotnet restore

# Copy the rest of the source and publish a framework-dependent release build.
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — runtime image
# The ASP.NET runtime image is much smaller than the SDK image (~220 MB vs ~900 MB).
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
# ASPNETCORE_URLS tells Kestrel which address/port to bind to inside the container.
# `http://+:8080` means: listen on HTTP port 8080 on all container interfaces, so
# Docker port mapping like `5188:8080` can forward traffic from the host into the app.
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

# Port the app listens on inside the container (HTTP).
# The connection string is injected at runtime via the environment variable
# ConnectionStrings__TodoContext, so no appsettings change is needed.
EXPOSE 8080

ENTRYPOINT ["dotnet", "TodoApi.dll"]
