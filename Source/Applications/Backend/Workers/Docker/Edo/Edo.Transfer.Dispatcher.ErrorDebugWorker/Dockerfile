#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/Edo/Edo.Transfer.Dispatcher.ErrorDebugWorker/Edo.Transfer.Dispatcher.ErrorDebugWorker.csproj", "Applications/Backend/Workers/Docker/Edo/Edo.Transfer.Dispatcher.ErrorDebugWorker/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/Edo/Edo.Transfer.Dispatcher.ErrorDebugWorker/Edo.Transfer.Dispatcher.ErrorDebugWorker.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/Edo/Edo.Transfer.Dispatcher.ErrorDebugWorker"
RUN dotnet build "./Edo.Transfer.Dispatcher.ErrorDebugWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Edo.Transfer.Dispatcher.ErrorDebugWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Edo.Transfer.Dispatcher.ErrorDebugWorker.dll"]