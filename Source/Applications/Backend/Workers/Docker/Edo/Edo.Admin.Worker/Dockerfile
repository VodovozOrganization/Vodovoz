﻿# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/Edo/Edo.Admin.Worker/Edo.Admin.Worker.csproj", "Applications/Backend/Workers/Docker/Edo/Edo.Admin.Worker/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Admin/Edo.Admin.csproj", "Libraries/Core/Backend/Edo/Edo.Admin/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Common/Edo.Common.csproj", "Libraries/Core/Backend/Edo/Edo.Common/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/GMap.NET/GMap.NET.Core/GMap.NET.Core.csproj", "Libraries/External/GMap.NET/GMap.NET.Core/"]
COPY ["Libraries/External/Gtk.DataBindings/System.Data.Bindings/System.Data.Bindings.csproj", "Libraries/External/Gtk.DataBindings/System.Data.Bindings/"]
COPY ["Libraries/External/QSProjects/Modules/QS.Banks/QS.Banks.csproj", "Libraries/External/QSProjects/Modules/QS.Banks/"]
COPY ["Libraries/External/QSProjects/QS.Extensions.Observable/QS.Extensions.Observable.csproj", "Libraries/External/QSProjects/QS.Extensions.Observable/"]
COPY ["Libraries/External/QSProjects/Modules/QS.BusinessCommon/QS.BusinessCommon.csproj", "Libraries/External/QSProjects/Modules/QS.BusinessCommon/"]
COPY ["Libraries/External/QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "Libraries/External/QSProjects/QS.HistoryLog/"]
COPY ["Libraries/Core/Infrastructure/Core.Infrastructure/Core.Infrastructure.csproj", "Libraries/Core/Infrastructure/Core.Infrastructure/"]
COPY ["Libraries/Core/Infrastructure/VodovozInfrastructure/VodovozInfrastructure.csproj", "Libraries/Core/Infrastructure/VodovozInfrastructure/"]
COPY ["Libraries/External/QSProjects/QS.Project/QS.Project.csproj", "Libraries/External/QSProjects/QS.Project/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Binding.Core/Gamma.Binding.Core.csproj", "Libraries/External/QSProjects/Binding/Gamma.Binding.Core/"]
COPY ["Libraries/External/QSProjects/QS.BaseParameters/QS.BaseParameters.csproj", "Libraries/External/QSProjects/QS.BaseParameters/"]
COPY ["Libraries/External/QSProjects/QS.Project.Desktop/QS.Project.Desktop.csproj", "Libraries/External/QSProjects/QS.Project.Desktop/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting/QS.ErrorReporting.csproj", "Libraries/External/QSProjects/QS.ErrorReporting/"]
COPY ["Libraries/External/QSProjects/QS.ErrorReporting.Client/QS.ErrorReporting.Client.csproj", "Libraries/External/QSProjects/QS.ErrorReporting.Client/"]
COPY ["Libraries/External/QSProjects/QS.Report/QS.Report.csproj", "Libraries/External/QSProjects/QS.Report/"]
COPY ["Libraries/External/My-FyiReporting/RdlEngine/RdlEngine.csproj", "Libraries/External/My-FyiReporting/RdlEngine/"]
COPY ["Libraries/External/My-FyiReporting/DataProviders/DataProviders.csproj", "Libraries/External/My-FyiReporting/DataProviders/"]
COPY ["Libraries/Core/Backend/Infrastructure/ApiClientProvider/ApiClientProvider.csproj", "Libraries/Core/Backend/Infrastructure/ApiClientProvider/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/TrueMarkApi.Client/TrueMarkApi.Client.csproj", "Libraries/Core/Backend/TrueMarkApi.Client/"]
COPY ["Libraries/Core/Backend/TrueMark/TrueMark.Contracts/TrueMark.Contracts.csproj", "Libraries/Core/Backend/TrueMark/TrueMark.Contracts/"]
COPY ["Libraries/Core/Backend/TrueMark/TrueMark.Codes.Pool/TrueMark.Codes.Pool.csproj", "Libraries/Core/Backend/TrueMark/TrueMark.Codes.Pool/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Transport/Edo.Transport.csproj", "Libraries/Core/Backend/Edo/Edo.Transport/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Contracts/Edo.Contracts.csproj", "Libraries/Core/Backend/Edo/Edo.Contracts/"]
COPY ["Libraries/Core/Backend/Infrastructure/BackendInfrastructure/BackendInfrastructure.csproj", "Libraries/Core/Backend/Infrastructure/BackendInfrastructure/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings.Database/Vodovoz.Settings.Database.csproj", "Libraries/Core/Business/Vodovoz.Settings.Database/"]
COPY ["Libraries/External/QSProjects/QS.Attachments/QS.Attachments.csproj", "Libraries/External/QSProjects/QS.Attachments/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/Edo/Edo.Admin.Worker/Edo.Admin.Worker.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/Edo/Edo.Admin.Worker"
RUN dotnet build "./Edo.Admin.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Edo.Admin.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Edo.Admin.Worker.dll"]
