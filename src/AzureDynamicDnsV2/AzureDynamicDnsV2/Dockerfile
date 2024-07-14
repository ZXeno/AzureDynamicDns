FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AzureDynamicDnsV2/AzureDynamicDnsV2.csproj", "AzureDynamicDnsV2/"]
RUN dotnet restore "AzureDynamicDnsV2/AzureDynamicDnsV2.csproj"
COPY . .
WORKDIR "/src/AzureDynamicDnsV2"
RUN dotnet build "AzureDynamicDnsV2.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AzureDynamicDnsV2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzureDynamicDnsV2.dll"]