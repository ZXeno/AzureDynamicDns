FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN  mkdir /app && chmod 777 /app
WORKDIR /app
COPY . .
RUN dotnet restore AzureDynamicDnsV2.csproj
RUN dotnet publish "AzureDynamicDnsV2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AzureDynamicDnsV2.dll"]
