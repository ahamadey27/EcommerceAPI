# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file and restore dependencies
COPY EcommerceAPI.csproj .
RUN dotnet restore EcommerceAPI.csproj

# Copy everything else and build the project (not solution)
COPY . .
RUN dotnet publish EcommerceAPI.csproj -c Release -o /app --no-restore /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]
