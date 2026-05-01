FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EsportsApp.API/EsportsApp.API.csproj", "EsportsApp.API/"]
COPY ["EsportsApp.Application/EsportsApp.Application.csproj", "EsportsApp.Application/"]
COPY ["EsportsApp.Domain/EsportsApp.Domain.csproj", "EsportsApp.Domain/"]
COPY ["EsportsApp.Infrastructure/EsportsApp.Infrastructure.csproj", "EsportsApp.Infrastructure/"]
RUN dotnet restore "EsportsApp.API/EsportsApp.API.csproj"
COPY . .
WORKDIR "/src/EsportsApp.API"
RUN dotnet build "EsportsApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EsportsApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EsportsApp.API.dll"]
