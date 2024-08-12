#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/WebAPI/WarehouseApi/WarehouseApi.csproj", "Applications/Backend/WebAPI/WarehouseApi/"]
RUN dotnet restore "./Applications/Backend/WebAPI/WarehouseApi/WarehouseApi.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/WarehouseApi"
RUN dotnet build "./WarehouseApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WarehouseApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WarehouseApi.dll"]