#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

ARG temp_dir="/tmp/cprocsp_packages"
RUN mkdir -p $temp_dir
# необходимо скачать со страницы https://www.cryptopro.ru/products/csp/downloads
COPY ["Net6/TrueMarkApi/CryptoProCspLocalPackages/Distrib/linux-amd64_deb.tgz", "$temp_dir"]

RUN cd "$temp_dir" && \
    tar -xf linux-amd64_deb.tgz && \
	linux-amd64_deb/install.sh && \
	rm -rf "$temp_dir"


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Net6/Nuget/packages", "Net6/Nuget/packages/"]
COPY ["Net6/NuGet.Config", "Net6/"]
COPY ["Net6/TrueMarkApi/TrueMarkApi.csproj", "Net6/TrueMarkApi/"]
RUN dotnet restore "./Net6/TrueMarkApi/TrueMarkApi.csproj"
COPY . .
WORKDIR "/src/Net6/TrueMarkApi"
RUN dotnet build "./TrueMarkApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrueMarkApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrueMarkApi.dll"]
