FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV SolutionDir /src
WORKDIR /src
COPY ["common/Authorization/Authorization.csproj", "common/Authorization/"]
COPY ["common/Extensions/Extensions.csproj", "common/Extensions/"]
COPY ["common/Extensions.AspNet/Extensions.AspNet.csproj", "common/Extensions.AspNet/"]
COPY ["common/Monitoring/Monitoring.csproj", "common/Monitoring/"]
COPY ["common/Monitoring.HealthChecks.Kafka/Monitoring.HealthChecks.Kafka.csproj", "common/Monitoring.HealthChecks.Kafka/"]
COPY ["common/Monitoring.HealthChecks.Mongo/Monitoring.HealthChecks.Mongo.csproj", "common/Monitoring.HealthChecks.Mongo/"]
COPY ["common/Monitoring.HealthChecks.Redis/Monitoring.HealthChecks.Redis.csproj", "common/Monitoring.HealthChecks.Redis/"]
COPY ["common/Persistence.Mongo/Persistence.Mongo.csproj", "common/Persistence.Mongo/"]
COPY ["status/Status.Common/Status.Common.csproj", "status/Status.Common/"]
COPY ["status/Status.Common.Messaging/Status.Common.Messaging.csproj", "status/Status.Common.Messaging/"]
COPY ["status/Status.Speed/Status.Speed.csproj", "status/Status.Speed/"]
COPY ["Acyclica/AcyclicaService.csproj", "Acyclica/"]
COPY ["AcyclicaConfigRepository/AcyclicaConfigRepository.csproj", "AcyclicaConfigRepository/"]
COPY ["AcyclicaServiceLibrary/AcyclicaServiceLibrary.csproj", "AcyclicaServiceLibrary/"]
COPY ["EntityService/EntityService.csproj", "EntityService/"]
COPY ["SegmentCollectionLibrary/SegmentCollectionLibrary.csproj", "SegmentCollectionLibrary/"]
RUN dotnet restore "Acyclica/AcyclicaService.csproj"
COPY . .
WORKDIR "/src/Acyclica"
RUN dotnet build "AcyclicaService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcyclicaService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AcyclicaService.dll"]