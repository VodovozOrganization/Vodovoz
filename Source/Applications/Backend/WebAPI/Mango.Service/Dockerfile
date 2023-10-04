#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Applications/Backend/WebAPI/Mango.Service/Mango.Service.csproj", "Applications/Backend/WebAPI/Mango.Service/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Backend/Mango/Mango.CallsPublishing/Mango.CallsPublishing.csproj", "Libraries/Core/Backend/Mango/Mango.CallsPublishing/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Core/Mango.Core.csproj", "Libraries/Core/Backend/Mango/Mango.Core/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Client/Mango.Client.csproj", "Libraries/Core/Backend/Mango/Mango.Client/"]
COPY ["Libraries/Core/Backend/Mango/MangoClient/Mango.Api.csproj", "Libraries/Core/Backend/Mango/MangoClient/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
RUN dotnet restore "Applications/Backend/WebAPI/Mango.Service/Mango.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/Mango.Service"
RUN dotnet build "Mango.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mango.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mango.Service.dll"]
