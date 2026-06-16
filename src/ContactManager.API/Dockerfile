# Multi-stage build for the ContactManager API.
# Build context is the repository root (see docker-compose build.context).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the nuget config so restore uses the public feed (matches local dev).
COPY nuget.config ./

# Copy csproj files first for layer-cached restore.
COPY src/ContactManager.Domain/ContactManager.Domain.csproj                         src/ContactManager.Domain/
COPY src/ContactManager.Application/ContactManager.Application.csproj               src/ContactManager.Application/
COPY src/ContactManager.Infrastructure.Data/ContactManager.Infrastructure.Data.csproj       src/ContactManager.Infrastructure.Data/
COPY src/ContactManager.Infrastructure.Identity/ContactManager.Infrastructure.Identity.csproj src/ContactManager.Infrastructure.Identity/
COPY src/ContactManager.Infrastructure.IoC/ContactManager.Infrastructure.IoC.csproj         src/ContactManager.Infrastructure.IoC/
COPY src/ContactManager.API/ContactManager.API.csproj                               src/ContactManager.API/
RUN dotnet restore src/ContactManager.API/ContactManager.API.csproj

# Copy the rest of the source and publish.
COPY src/ ./src/
RUN dotnet publish src/ContactManager.API/ContactManager.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Listen on 8080 inside the container (non-privileged).
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ContactManager.API.dll"]
