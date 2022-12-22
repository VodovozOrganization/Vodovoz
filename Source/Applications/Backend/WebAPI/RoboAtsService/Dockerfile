#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["RoboatsService/RoboatsService.csproj", "RoboatsService/"]
RUN dotnet restore "RoboatsService/RoboatsService.csproj"
COPY . .
WORKDIR "/src/RoboatsService"
RUN dotnet build "RoboatsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RoboatsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoboatsService.dll"]
