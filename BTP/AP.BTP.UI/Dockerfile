# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AP.BTP.UI/AP.BTP.UI.csproj", "AP.BTP.UI/"]
COPY ["AP.BTP.Application/AP.BTP.Application.csproj", "AP.BTP.Application/"]
COPY ["AP.BTP.Domain/AP.BTP.Domain.csproj", "AP.BTP.Domain/"]
COPY ["AP.BTP.Infrastructure/AP.BTP.Infrastructure.csproj", "AP.BTP.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AP.BTP.UI/AP.BTP.UI.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/AP.BTP.UI"
RUN dotnet build "AP.BTP.UI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AP.BTP.UI.csproj" -c Release -o /app/publish

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Create the required Files directory before copying publish output
RUN mkdir -p /app/wwwroot/Files/GroundPlans/
COPY --from=publish /app/publish .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 8080
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "AP.BTP.UI.dll"]