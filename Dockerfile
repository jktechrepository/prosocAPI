# Use the official .NET 6.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 6.0 SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["ProsocAPI/ProsocAPI.csproj", "ProsocAPI/"]
COPY ["Prosoc.Tests.Unit/Prosoc.Tests.Unit.csproj", "Prosoc.Tests.Unit/"]
COPY ["Prosoc.Tests.Integration/Prosoc.Tests.Integration.csproj", "Prosoc.Tests.Integration/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Restore NuGet packages
RUN dotnet restore "ProsocAPI/ProsocAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ProsocAPI"
RUN dotnet build "ProsocAPI.csproj" -c Release -o /app/build

# Run tests during build (optional - can be commented out for faster builds)
# FROM build AS test
# WORKDIR "/src"
# RUN dotnet test "Prosoc.Tests.Unit/Prosoc.Tests.Unit.csproj" --no-build --configuration Release --results-directory /test-results
# RUN dotnet test "Prosoc.Tests.Integration/Prosoc.Tests.Integration.csproj" --no-build --configuration Release --results-directory /test-results

# Publish the application
FROM build AS publish
RUN dotnet publish "ProsocAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ProsocAPI.dll"]
