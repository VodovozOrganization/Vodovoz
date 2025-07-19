# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM base AS final
WORKDIR /app
COPY ["./bin/Release/net5.0/publish/linux-x64", "."]
ENTRYPOINT ["dotnet", "EdoDocumentFlowUpdater.dll"]
