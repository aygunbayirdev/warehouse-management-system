FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ ./src/

RUN dotnet restore src/WMS.Api/WMS.Api.csproj
RUN dotnet publish src/WMS.Api/WMS.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WMS.Api.dll"]
