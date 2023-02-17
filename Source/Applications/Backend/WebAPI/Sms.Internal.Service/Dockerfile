#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/WebAPI/Sms.Internal.Service/Sms.Internal.Service.csproj", "Applications/Backend/WebAPI/Sms.Internal.Service/"]
RUN dotnet restore "Applications/Backend/WebAPI/Sms.Internal.Service/Sms.Internal.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/Sms.Internal.Service"
RUN dotnet build "Sms.Internal.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sms.Internal.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sms.Internal.Service.dll"]
