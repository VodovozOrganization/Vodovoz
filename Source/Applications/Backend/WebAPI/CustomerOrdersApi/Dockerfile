FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/WebAPI/CustomerOrdersApi/CustomerOrdersApi.csproj", "Applications/Backend/WebAPI/CustomerOrdersApi/"]
RUN dotnet restore "Applications/Backend/WebAPI/CustomerOrdersApi/CustomerOrdersApi.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/WebAPI/CustomerOrdersApi"
RUN dotnet build "CustomerOrdersApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CustomerOrdersApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerOrdersApi.dll"]
