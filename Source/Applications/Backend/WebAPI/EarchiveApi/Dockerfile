#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/WebAPI/EarchiveApi/EarchiveApi.csproj", "Applications/Backend/WebAPI/EarchiveApi/"]
RUN dotnet restore "Applications/Backend/WebAPI/EarchiveApi/EarchiveApi.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/EarchiveApi"
RUN dotnet build "EarchiveApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EarchiveApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EarchiveApi.dll"]
EXPOSE 7101
