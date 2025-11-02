#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Services/Pacs.Admin.Service/Pacs.Admin.Service.csproj", "Applications/Backend/Services/Pacs.Admin.Service/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Admin.Server/Pacs.Admin.Server.csproj", "Libraries/Core/Backend/Pacs/Pacs.Admin.Server/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/GMap.NET/GMap.NET.Core/GMap.NET.Core.csproj", "Libraries/External/GMap.NET/GMap.NET.Core/"]
COPY ["Libraries/External/QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "Libraries/External/QSProjects/QS.HistoryLog/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Core/Pacs.Core.csproj", "Libraries/Core/Backend/Pacs/Pacs.Core/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/External/QSProjects/QS.Project/QS.Project.csproj", "Libraries/External/QSProjects/QS.Project/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Binding.Core/Gamma.Binding.Core.csproj", "Libraries/External/QSProjects/Binding/Gamma.Binding.Core/"]
COPY ["Libraries/External/QSProjects/QS.BaseParameters/QS.BaseParameters.csproj", "Libraries/External/QSProjects/QS.BaseParameters/"]
COPY ["Libraries/External/QSProjects/QS.Project.Desktop/QS.Project.Desktop.csproj", "Libraries/External/QSProjects/QS.Project.Desktop/"]
COPY ["Libraries/External/Gtk.DataBindings/System.Data.Bindings/System.Data.Bindings.csproj", "Libraries/External/Gtk.DataBindings/System.Data.Bindings/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting/QS.ErrorReporting.csproj", "Libraries/External/QSProjects/QS.ErrorReporting/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting.Client/QS.ErrorReporting.Client.csproj", "Libraries/External/QSProjects/QS.ErrorReporting.Client/"]
COPY ["Libraries/Core/Infrastructure/Core.Infrastructure/Core.Infrastructure.csproj", "Libraries/Core/Infrastructure/Core.Infrastructure/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings.Database/Vodovoz.Settings.Database.csproj", "Libraries/Core/Business/Vodovoz.Settings.Database/"]
RUN dotnet restore "./Applications/Backend/Services/Pacs.Admin.Service/Pacs.Admin.Service.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Services/Pacs.Admin.Service"
RUN dotnet build "./Pacs.Admin.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pacs.Admin.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pacs.Admin.Service.dll"]