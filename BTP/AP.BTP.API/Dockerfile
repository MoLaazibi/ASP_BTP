# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AP.BTP.API/AP.BTP.API.csproj", "AP.BTP.API/"]
COPY ["AP.BTP.Application/AP.BTP.Application.csproj", "AP.BTP.Application/"]
COPY ["AP.BTP.Domain/AP.BTP.Domain.csproj", "AP.BTP.Domain/"]
COPY ["AP.BTP.Infrastructure/AP.BTP.Infrastructure.csproj", "AP.BTP.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AP.BTP.API/AP.BTP.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/AP.BTP.API"
RUN dotnet build "AP.BTP.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AP.BTP.API.csproj" -c Release -o /app/publish

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir -p /app/wwwroot/Files/
COPY --from=publish /app/publish .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 8081
EXPOSE 8081

# Set the entry point
ENTRYPOINT ["dotnet", "AP.BTP.API.dll"]