# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/EdoContactsUpdater.csproj", "Applications/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/EdoContactsUpdater.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EdoServices/EdoContactsUpdater"
RUN dotnet build "./EdoContactsUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EdoContactsUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EdoContactsUpdater.dll"]