FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY SpaceExplorer.slnx .
COPY src/ src/

RUN dotnet restore
RUN dotnet publish src/SpaceExplorer.API/SpaceExplorer.API.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SpaceExplorer.API.dll"]
