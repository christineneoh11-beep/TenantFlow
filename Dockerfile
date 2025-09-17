# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.sln .
COPY TenantFlow.Web/*.csproj ./TenantFlow/
RUN dotnet restore

# Copy the rest of the code
COPY . .
WORKDIR /src/TenantFlow

# Publish the app
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port for Render
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "TenantFlow.dll"]
