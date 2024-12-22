# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["Applications/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "Applications/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer"
RUN dotnet build "./TaxcomEdoConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaxcomEdoConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaxcomEdoConsumer.dll"]