# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy sln and csproj files
COPY *.sln .
COPY TenantFlow.Web.csproj ./
RUN dotnet restore TenantFlow.Web.csproj

# Copy the rest of the code
COPY . .
WORKDIR /src
RUN dotnet publish TenantFlow.Web.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TenantFlow.Web.dll"]
