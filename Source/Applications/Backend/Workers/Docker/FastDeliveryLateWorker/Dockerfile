#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM base AS final
WORKDIR /app
COPY ["./bin/Release/net5.0/linux-x64/publish", "."]
ENTRYPOINT ["dotnet", "FastDeliveryLateWorker.dll"]
