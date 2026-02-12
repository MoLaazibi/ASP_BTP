# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for restore
COPY ["AP.BTP.MobileUI/AP.BTP.MobileUI.csproj", "AP.BTP.MobileUI/"]
COPY ["AP.BTP.Application/AP.BTP.Application.csproj", "AP.BTP.Application/"]

# Restore
RUN dotnet restore "AP.BTP.MobileUI/AP.BTP.MobileUI.csproj"

# Copy source
COPY . .

# Publish WebAssembly output
WORKDIR /src/AP.BTP.MobileUI
RUN dotnet publish "AP.BTP.MobileUI.csproj" -c Release -o /app/publish

# Runtime stage using nginx to serve static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy SPA files
COPY --from=build /app/publish/wwwroot .

# Nginx config for SPA fallback
COPY AP.BTP.MobileUI/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]