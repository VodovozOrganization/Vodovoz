#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/Services/Pacs.Calls.Service/Pacs.Calls.Service.csproj", "Applications/Backend/Services/Pacs.Calls.Service/"]
RUN dotnet restore "Applications/Backend/Services/Pacs.Calls.Service/Pacs.Calls.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Services/Pacs.Calls.Service"
RUN dotnet build "Pacs.Calls.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pacs.Calls.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pacs.Calls.Service.dll"]