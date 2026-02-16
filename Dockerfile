# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Swimago.API/Swimago.API.csproj", "src/Swimago.API/"]
COPY ["src/Swimago.Application/Swimago.Application.csproj", "src/Swimago.Application/"]
COPY ["src/Swimago.Domain/Swimago.Domain.csproj", "src/Swimago.Domain/"]
COPY ["src/Swimago.Infrastructure/Swimago.Infrastructure.csproj", "src/Swimago.Infrastructure/"]

RUN dotnet restore "src/Swimago.API/Swimago.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Swimago.API"
RUN dotnet build "Swimago.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Swimago.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Swimago.API.dll"]
