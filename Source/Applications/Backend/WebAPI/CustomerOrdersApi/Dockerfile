FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM base AS final
WORKDIR /app
COPY ["./bin/Release/net5.0_publish", "."]
ENTRYPOINT ["dotnet", "CustomerOrdersApi.dll"]
