#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

ARG temp_dir="/tmp/cprocsp_packages"
RUN mkdir -p $temp_dir
# необходимо скачать со страницы https://www.cryptopro.ru/products/csp/downloads
COPY ["Applications/Backend/WebAPI/TrueMark.Api/CryptoProCspLocalPackages/Distrib/linux-amd64_deb.tgz", "$temp_dir"]

RUN cd "$temp_dir" && \
    tar -xf linux-amd64_deb.tgz && \
	linux-amd64_deb/install.sh && \
	rm -rf "$temp_dir"

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Packages", "Packages/"]
COPY ["Applications/Backend/WebAPI/TrueMark.Api/TrueMark.Api.csproj", "Applications/Backend/WebAPI/TrueMark.Api/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]

COPY . .

RUN dotnet restore "./Applications/Backend/WebAPI/TrueMark.Api/TrueMark.Api.csproj"
WORKDIR "/src/Applications/Backend/WebAPI/TrueMark.Api"
RUN dotnet build "./TrueMark.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrueMark.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrueMark.Api.dll"]
