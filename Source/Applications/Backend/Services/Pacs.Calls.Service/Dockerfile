#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/Services/Pacs.Calls.Service/Pacs.Calls.Service.csproj", "Applications/Backend/Services/Pacs.Calls.Service/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.MangoCalls/Pacs.MangoCalls.csproj", "Libraries/Core/Backend/Pacs/Pacs.MangoCalls/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "Libraries/External/QSProjects/QS.HistoryLog/"]
COPY ["Libraries/Core/Backend/Mango/Mango.CallsPublishing/Mango.CallsPublishing.csproj", "Libraries/Core/Backend/Mango/Mango.CallsPublishing/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Core/Mango.Core.csproj", "Libraries/Core/Backend/Mango/Mango.Core/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Server/Pacs.Server.csproj", "Libraries/Core/Backend/Pacs/Pacs.Server/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Core/Pacs.Core.csproj", "Libraries/Core/Backend/Pacs/Pacs.Core/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings.Database/Vodovoz.Settings.Database.csproj", "Libraries/Core/Business/Vodovoz.Settings.Database/"]
COPY ["Libraries/Core/Infrastructure/Core.Infrastructure/Core.Infrastructure.csproj", "Libraries/Core/Infrastructure/Core.Infrastructure/"]
RUN dotnet restore "Applications/Backend/Services/Pacs.Calls.Service/Pacs.Calls.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Services/Pacs.Calls.Service"
RUN dotnet build "Pacs.Calls.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pacs.Calls.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pacs.Calls.Service.dll"]