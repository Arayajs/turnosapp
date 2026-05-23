# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/TurnOS.API/TurnOS.API.csproj",           "src/TurnOS.API/"]
COPY ["src/TurnOS.Infrastructure/TurnOS.Infrastructure.csproj", "src/TurnOS.Infrastructure/"]
COPY ["src/TurnOS.Application/TurnOS.Application.csproj",       "src/TurnOS.Application/"]
COPY ["src/TurnOS.Domain/TurnOS.Domain.csproj",                 "src/TurnOS.Domain/"]
RUN dotnet restore "src/TurnOS.API/TurnOS.API.csproj"

COPY . .
WORKDIR "/src/src/TurnOS.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TurnOS.API.dll"]
