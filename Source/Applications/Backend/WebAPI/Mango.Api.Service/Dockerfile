#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/WebAPI/Mango.Api.Service/Mango.Api.Service.csproj", "Applications/Backend/WebAPI/Mango.Api.Service/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Api/Mango.Api.csproj", "Libraries/Core/Backend/Mango/Mango.Api/"]
COPY ["Libraries/Core/Backend/Mango/Mango.CallsPublishing/Mango.CallsPublishing.csproj", "Libraries/Core/Backend/Mango/Mango.CallsPublishing/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Core/Mango.Core.csproj", "Libraries/Core/Backend/Mango/Mango.Core/"]
RUN dotnet restore "./Applications/Backend/WebAPI/Mango.Api.Service/Mango.Api.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/Mango.Api.Service"
RUN dotnet build "./Mango.Api.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Mango.Api.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mango.Api.Service.dll"]
