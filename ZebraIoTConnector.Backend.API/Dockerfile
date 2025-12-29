FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ZebraIoTConnector.Backend.API/ZebraIoTConnector.Backend.API.csproj", "ZebraIoTConnector.Backend.API/"]
COPY ["ZebraIoTConnector.Client.MQTT.Console/ZebraIoTConnector.Client.MQTT.Console.csproj", "ZebraIoTConnector.Client.MQTT.Console/"]
COPY ["ZebraIoTConnector.Services/ZebraIoTConnector.Services.csproj", "ZebraIoTConnector.Services/"]
COPY ["ZebraIoTConnector.DomainModel/ZebraIoTConnector.DomainModel.csproj", "ZebraIoTConnector.DomainModel/"]
COPY ["ZebraIoTConnector.FXReaderInterface/ZebraIoTConnector.FXReaderInterface.csproj", "ZebraIoTConnector.FXReaderInterface/"]
COPY ["ZebraIoTConnector.Persistence/ZebraIoTConnector.Persistence.csproj", "ZebraIoTConnector.Persistence/"]
RUN dotnet restore "ZebraIoTConnector.Backend.API/ZebraIoTConnector.Backend.API.csproj"
COPY . .
WORKDIR "/src/ZebraIoTConnector.Backend.API"
RUN dotnet build "ZebraIoTConnector.Backend.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZebraIoTConnector.Backend.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZebraIoTConnector.Backend.API.dll"]
