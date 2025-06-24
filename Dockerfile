# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY EcommerceAPI/EcommerceAPI.csproj ./EcommerceAPI/
RUN dotnet restore ./EcommerceAPI/EcommerceAPI.csproj

# Copy the source code
COPY . .

# Build the application
RUN dotnet publish EcommerceAPI/EcommerceAPI.csproj -c Release -o out

# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application
COPY --from=build /app/out .

# Expose the port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://*:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]
