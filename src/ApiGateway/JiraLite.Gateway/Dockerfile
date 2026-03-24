FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore
COPY ["src/ApiGateway/JiraLite.Gateway/JiraLite.Gateway.csproj", "src/ApiGateway/JiraLite.Gateway/"]
RUN dotnet restore "src/ApiGateway/JiraLite.Gateway/JiraLite.Gateway.csproj"

# Copy source code
COPY ["src/ApiGateway/JiraLite.Gateway/", "src/ApiGateway/JiraLite.Gateway/"]

# Build
WORKDIR "/src/src/ApiGateway/JiraLite.Gateway"
RUN dotnet build "JiraLite.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JiraLite.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JiraLite.Gateway.dll"]
