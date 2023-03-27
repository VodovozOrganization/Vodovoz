FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/ExternalCounterpartyAssignNotifier/ExternalCounterpartyAssignNotifier.csproj", "Applications/Backend/Workers/Docker/ExternalCounterpartyAssignNotifier/"]
RUN dotnet restore "Applications/Backend/Workers/Docker/ExternalCounterpartyAssignNotifier/ExternalCounterpartyAssignNotifier.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/ExternalCounterpartyAssignNotifier"
RUN dotnet build "ExternalCounterpartyAssignNotifier.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExternalCounterpartyAssignNotifier.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExternalCounterpartyAssignNotifier.dll"]
