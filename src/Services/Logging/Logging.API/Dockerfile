FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Logging/Logging.API/Logging.API.csproj", "src/Services/Logging/Logging.API/"]
COPY ["src/Services/Logging/Logging.Application/Logging.Application.csproj", "src/Services/Logging/Logging.Application/"]
COPY ["src/Services/Logging/Logging.Domain/Logging.Domain.csproj", "src/Services/Logging/Logging.Domain/"]
COPY ["src/Services/Logging/Logging.Infrastructure/Logging.Infrastructure.csproj", "src/Services/Logging/Logging.Infrastructure/"]
COPY ["src/SharedKernel/JiraLite.Share.Contracts/JiraLite.Shared.Contracts.csproj", "src/SharedKernel/JiraLite.Share.Contracts/"]
COPY ["src/SharedKernel/JiraLite.Shared.Messaging/JiraLite.Shared.Messaging.csproj", "src/SharedKernel/JiraLite.Shared.Messaging/"]

RUN dotnet restore "src/Services/Logging/Logging.API/Logging.API.csproj"

# Copy source code
COPY ["src/Services/Logging/", "src/Services/Logging/"]
COPY ["src/SharedKernel/", "src/SharedKernel/"]

# Build
WORKDIR "/src/src/Services/Logging/Logging.API"
RUN dotnet build "Logging.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Logging.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Logging.API.dll"]
