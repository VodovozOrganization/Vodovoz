#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["UnsubscribePage/UnsubscribePage.csproj", "UnsubscribePage/"]
RUN dotnet restore "UnsubscribePage/UnsubscribePage.csproj"
COPY . .
WORKDIR "/src/UnsubscribePage"
RUN dotnet build "UnsubscribePage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UnsubscribePage.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UnsubscribePage.dll"]