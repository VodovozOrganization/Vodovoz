#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/Edo/Edo.Receipt.Api/Edo.Receipt.Api.csproj", "Applications/Backend/Workers/Docker/Edo/Edo.Receipt.Api/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Receipt.Callback/Edo.Receipt.Callback.csproj", "Libraries/Core/Backend/Edo/Edo.Receipt.Callback/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/External/QSProjects/Modules/QS.BusinessCommon/QS.BusinessCommon.csproj", "Libraries/External/QSProjects/Modules/QS.BusinessCommon/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project/QS.Project.csproj", "Libraries/External/QSProjects/QS.Project/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Binding.Core/Gamma.Binding.Core.csproj", "Libraries/External/QSProjects/Binding/Gamma.Binding.Core/"]
COPY ["Libraries/External/QSProjects/QS.BaseParameters/QS.BaseParameters.csproj", "Libraries/External/QSProjects/QS.BaseParameters/"]
COPY ["Libraries/External/QSProjects/QS.Extensions.Observable/QS.Extensions.Observable.csproj", "Libraries/External/QSProjects/QS.Extensions.Observable/"]
COPY ["Libraries/External/QSProjects/QS.Project.Desktop/QS.Project.Desktop.csproj", "Libraries/External/QSProjects/QS.Project.Desktop/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting/QS.ErrorReporting.csproj", "Libraries/External/QSProjects/QS.ErrorReporting/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting.Client/QS.ErrorReporting.Client.csproj", "Libraries/External/QSProjects/QS.ErrorReporting.Client/"]
COPY ["Libraries/Core/Infrastructure/Core.Infrastructure/Core.Infrastructure.csproj", "Libraries/Core/Infrastructure/Core.Infrastructure/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/GMap.NET/GMap.NET.Core/GMap.NET.Core.csproj", "Libraries/External/GMap.NET/GMap.NET.Core/"]
COPY ["Libraries/External/Gtk.DataBindings/System.Data.Bindings/System.Data.Bindings.csproj", "Libraries/External/Gtk.DataBindings/System.Data.Bindings/"]
COPY ["Libraries/External/QSProjects/Modules/QS.Banks/QS.Banks.csproj", "Libraries/External/QSProjects/Modules/QS.Banks/"]
COPY ["Libraries/External/QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "Libraries/External/QSProjects/QS.HistoryLog/"]
COPY ["Libraries/Core/Infrastructure/VodovozInfrastructure/VodovozInfrastructure.csproj", "Libraries/Core/Infrastructure/VodovozInfrastructure/"]
COPY ["Libraries/External/QSProjects/QS.Report/QS.Report.csproj", "Libraries/External/QSProjects/QS.Report/"]
COPY ["Libraries/External/My-FyiReporting/RdlEngine/RdlEngine.csproj", "Libraries/External/My-FyiReporting/RdlEngine/"]
COPY ["Libraries/External/My-FyiReporting/DataProviders/DataProviders.csproj", "Libraries/External/My-FyiReporting/DataProviders/"]
COPY ["Libraries/Core/Backend/Infrastructure/ApiClientProvider/ApiClientProvider.csproj", "Libraries/Core/Backend/Infrastructure/ApiClientProvider/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings.Database/Vodovoz.Settings.Database.csproj", "Libraries/Core/Business/Vodovoz.Settings.Database/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Receipt/Edo.Receipt.csproj", "Libraries/Core/Backend/Edo/Edo.Receipt/"]
COPY ["Libraries/Core/Backend/ModulKassa/ModulKassa.csproj", "Libraries/Core/Backend/ModulKassa/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Transport/Edo.Transport.csproj", "Libraries/Core/Backend/Edo/Edo.Transport/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Contracts/Edo.Contracts.csproj", "Libraries/Core/Backend/Edo/Edo.Contracts/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/Edo/Edo.Receipt.Api/Edo.Receipt.Api.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/Edo/Edo.Receipt.Api"
RUN dotnet build "./Edo.Receipt.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Edo.Receipt.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Edo.Receipt.Api.dll"]