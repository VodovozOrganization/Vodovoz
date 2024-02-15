#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/Services/Pacs.Operator.Service/Pacs.Operator.Service.csproj", "Applications/Backend/Services/Pacs.Operator.Service/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Operator.Server/Pacs.Operator.Server.csproj", "Libraries/Core/Backend/Pacs/Pacs.Operator.Server/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Core/Pacs.Core.csproj", "Libraries/Core/Backend/Pacs/Pacs.Core/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "Libraries/External/QSProjects/QS.HistoryLog/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Server/Pacs.Server.csproj", "Libraries/Core/Backend/Pacs/Pacs.Server/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Applications/Backend/Services/Pacs.Admin.Service/Pacs.Admin.Service.csproj", "Applications/Backend/Services/Pacs.Admin.Service/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Admin.Server/Pacs.Admin.Server.csproj", "Libraries/Core/Backend/Pacs/Pacs.Admin.Server/"]
RUN dotnet restore "Applications/Backend/Services/Pacs.Operator.Service/Pacs.Operator.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Services/Pacs.Operator.Service"
RUN dotnet build "Pacs.Operator.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pacs.Operator.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pacs.Operator.Service.dll"]