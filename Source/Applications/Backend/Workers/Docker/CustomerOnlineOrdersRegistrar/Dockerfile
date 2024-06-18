FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM base AS final
WORKDIR /app
COPY ["./bin/Release/net5.0_publish", "."]
ENTRYPOINT ["dotnet", "CustomerOnlineOrdersRegistrar.dll"]

#FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
#WORKDIR /app

#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Applications/Backend/Workers/Docker/CustomerOnlineOrdersRegistrar/CustomerOnlineOrdersRegistrar.csproj", "Applications/Backend/Workers/Docker/CustomerOnlineOrdersRegistrar/"]
#RUN dotnet restore "Applications/Backend/Workers/Docker/CustomerOnlineOrdersRegistrar/CustomerOnlineOrdersRegistrar.csproj"
#COPY . .
#WORKDIR "/src/Applications/Backend/Workers/Docker/CustomerOnlineOrdersRegistrar"
#RUN dotnet build "CustomerOnlineOrdersRegistrar.csproj" -c $BUILD_CONFIGURATION -o /app/build

#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "CustomerOnlineOrdersRegistrar.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "CustomerOnlineOrdersRegistrar.dll"]
