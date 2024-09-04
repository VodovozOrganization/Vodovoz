FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/EdoDocumentsConsumer/EdoDocumentsConsumer.csproj", "Applications/Backend/Workers/Docker/EdoDocumentsConsumer/"]
RUN dotnet restore "Applications/Backend/Workers/Docker/EdoDocumentsConsumer/EdoDocumentsConsumer.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EdoDocumentsConsumer"
RUN dotnet build "EdoDocumentsConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EdoDocumentsConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EdoDocumentsConsumer.dll"]
