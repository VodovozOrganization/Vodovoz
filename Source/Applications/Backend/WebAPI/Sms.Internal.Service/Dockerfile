#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["/Services/Sms/Sms.Internal.Service/Sms.Internal.Service.csproj", "RepositoryRoot/Vodovoz/Services/Sms/Sms.Internal.Service/"]
RUN dotnet restore "RepositoryRoot/Vodovoz/Services/Sms/Sms.Internal.Service/Sms.Internal.Service.csproj"
COPY . /RepositoryRoot/Vodovoz/
COPY ../QSProjects/QS.Project.Core /RepositoryRoot/QSProjects/QS.Project.Core
WORKDIR "/src/RepositoryRoot/Vodovoz/Services/Sms/Sms.Internal.Service"
RUN dotnet build "Sms.Internal.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sms.Internal.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 9151
ENTRYPOINT ["dotnet", "Sms.Internal.Service.dll"]
