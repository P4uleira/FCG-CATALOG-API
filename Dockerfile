FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["src/FCG.Catalog.Api/FCG.Catalog.Api.csproj", "src/FCG.Catalog.Api/"]
COPY ["src/FCG.Catalog.Application/FCG.Catalog.Application.csproj", "src/FCG.Catalog.Application/"]
COPY ["src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "src/FCG.Catalog.Domain/"]
COPY ["src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "src/FCG.Catalog.Infrastructure/"]
COPY ["src/FCG.Catalog.Contracts/FCG.Catalog.Contracts.csproj", "src/FCG.Catalog.Contracts/"]

RUN dotnet restore \
    "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj"

FROM restore AS build

COPY . .

RUN dotnet publish \
    "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8081
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.Catalog.Api.dll"]