#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Libraries/Core/Backend/Edo/Edo.Receipt.Sender/Edo.Receipt.Sender.csproj", "Libraries/Core/Backend/Edo/Edo.Receipt.Sender/"]
RUN dotnet restore "./Libraries/Core/Backend/Edo/Edo.Receipt.Sender/Edo.Receipt.Sender.csproj"
COPY . .
WORKDIR "/src/Libraries/Core/Backend/Edo/Edo.Receipt.Sender"
RUN dotnet build "./Edo.Receipt.Sender.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Edo.Receipt.Sender.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Edo.Receipt.Sender.dll"]