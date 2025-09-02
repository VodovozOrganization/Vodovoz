FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "Applications/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Docflow.Taxcom/Edo.Docflow.Taxcom.csproj", "Libraries/Core/Backend/Edo/Edo.Docflow.Taxcom/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Domain/Vodovoz.Core.Domain.csproj", "Libraries/Core/Business/Vodovoz.Core.Domain/"]
COPY ["Libraries/External/GMap.NET/GMap.NET.Core/GMap.NET.Core.csproj", "Libraries/External/GMap.NET/GMap.NET.Core/"]
COPY ["Libraries/External/Gtk.DataBindings/System.Data.Bindings/System.Data.Bindings.csproj", "Libraries/External/Gtk.DataBindings/System.Data.Bindings/"]
COPY ["Libraries/External/QSProjects/Modules/QS.Banks/QS.Banks.csproj", "Libraries/External/QSProjects/Modules/QS.Banks/"]
COPY ["Libraries/External/QSProjects/QS.Project.Core/QS.Project.Core.csproj", "Libraries/External/QSProjects/QS.Project.Core/"]
COPY ["Libraries/External/QSProjects/Binding/Gamma.Utilities/Gamma.Utilities.csproj", "Libraries/External/QSProjects/Binding/Gamma.Utilities/"]
COPY ["Libraries/External/QSProjects/QS.Project.Abstract/QS.Project.Abstract.csproj", "Libraries/External/QSProjects/QS.Project.Abstract/"]
COPY ["Libraries/External/QSProjects/QS.Utilities/QS.Utilities.csproj", "Libraries/External/QSProjects/QS.Utilities/"]
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
COPY ["Libraries/Core/Backend/TaxcomEdo/TaxcomEdo.Client/TaxcomEdo.Client.csproj", "Libraries/Core/Backend/TaxcomEdo/TaxcomEdo.Client/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Contracts/Edo.Contracts.csproj", "Libraries/Core/Backend/Edo/Edo.Contracts/"]
COPY ["Libraries/Core/Backend/TaxcomEdo/TaxcomEdo.Contracts/TaxcomEdo.Contracts.csproj", "Libraries/Core/Backend/TaxcomEdo/TaxcomEdo.Contracts/"]
COPY ["Libraries/External/DateTime/DateTimeHelpers/DateTimeHelpers.csproj", "Libraries/External/DateTime/DateTimeHelpers/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Contracts.Xml/Edo.Contracts.Xml/Edo.Contracts.Xml.csproj", "Libraries/Core/Backend/Edo/Edo.Contracts.Xml/Edo.Contracts.Xml/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Docflow/Edo.Docflow.csproj", "Libraries/Core/Backend/Edo/Edo.Docflow/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data/Vodovoz.Core.Data.csproj", "Libraries/Core/Business/Vodovoz.Core.Data/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Transport/Edo.Transport.csproj", "Libraries/Core/Backend/Edo/Edo.Transport/"]
COPY ["Libraries/Core/Backend/HealthCheck/VodovozHealthCheck.csproj", "Libraries/Core/Backend/HealthCheck/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Backend/Vodovoz.Zabbix.Sender/Vodovoz.Zabbix.Sender.csproj", "Libraries/Core/Backend/Vodovoz.Zabbix.Sender/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings.Database/Vodovoz.Settings.Database.csproj", "Libraries/Core/Business/Vodovoz.Settings.Database/"]
COPY ["Libraries/Core/Business/Vodovoz.Application/Vodovoz.Application.csproj", "Libraries/Core/Business/Vodovoz.Application/"]
COPY ["Libraries/Core/Backend/DriverApi.Notifications.Client/DriverApi.Notifications.Client.csproj", "Libraries/Core/Backend/DriverApi.Notifications.Client/"]
COPY ["Libraries/Core/Business/VodovozBusiness/VodovozBusiness.csproj", "Libraries/Core/Business/VodovozBusiness/"]
COPY ["Libraries/External/QSProjects/QS.Attachments/QS.Attachments.csproj", "Libraries/External/QSProjects/QS.Attachments/"]
COPY ["Libraries/External/QSProjects/QS.DocTemplates/QS.DocTemplates.csproj", "Libraries/External/QSProjects/QS.DocTemplates/"]
COPY ["Libraries/External/QSProjects/QS.Osrm/QS.Osrm.csproj", "Libraries/External/QSProjects/QS.Osrm/"]
COPY ["Libraries/Core/Backend/CashReceipt/CashReceiptApi.Client.Framework/CashReceiptApi.Client.Framework.csproj", "Libraries/Core/Backend/CashReceipt/CashReceiptApi.Client.Framework/"]
COPY ["Libraries/Core/Backend/DriverApi.Contracts/DriverApi.Contracts.csproj", "Libraries/Core/Backend/DriverApi.Contracts/"]
COPY ["Libraries/Core/Backend/Edo/Edo.Common/Edo.Common.csproj", "Libraries/Core/Backend/Edo/Edo.Common/"]
COPY ["Libraries/Core/Backend/TrueMarkApi.Client/TrueMarkApi.Client.csproj", "Libraries/Core/Backend/TrueMarkApi.Client/"]
COPY ["Libraries/Core/Backend/TrueMark/TrueMark.Contracts/TrueMark.Contracts.csproj", "Libraries/Core/Backend/TrueMark/TrueMark.Contracts/"]
COPY ["Libraries/Core/Backend/TrueMark/TrueMark.Codes.Pool/TrueMark.Codes.Pool.csproj", "Libraries/Core/Backend/TrueMark/TrueMark.Codes.Pool/"]
COPY ["Libraries/Core/Backend/TrueMark/TrueMark.Codes/TrueMark.Codes.csproj", "Libraries/Core/Backend/TrueMark/TrueMark.Codes/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Core/Pacs.Core.csproj", "Libraries/Core/Backend/Pacs/Pacs.Core/"]
COPY ["Libraries/Core/Backend/Sms/Sms.Internal.Client.Famework/Sms.Internal.Client.Framework.csproj", "Libraries/Core/Backend/Sms/Sms.Internal.Client.Famework/"]
COPY ["Libraries/Core/Infrastructure/Vodovoz.RDL/Vodovoz.RDL.csproj", "Libraries/Core/Infrastructure/Vodovoz.RDL/"]
COPY ["Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/Vodovoz.Core.Data.NHibernate.csproj", "Libraries/Core/Business/Vodovoz.Core.Data.NHibernate/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Client/Mango.Client.csproj", "Libraries/Core/Backend/Mango/Mango.Client/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Core/Mango.Core.csproj", "Libraries/Core/Backend/Mango/Mango.Core/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Grpc.Client/Mango.Grpc.Client.csproj", "Libraries/Core/Backend/Mango/Mango.Grpc.Client/"]
COPY ["Libraries/Core/Backend/Mango/Mango.Grpc/Mango.Grpc.shproj", "Libraries/Core/Backend/Mango/Mango.Grpc/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Admin.Client/Pacs.Admin.Client.csproj", "Libraries/Core/Backend/Pacs/Pacs.Admin.Client/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Operator.Client/Pacs.Operator.Client.csproj", "Libraries/Core/Backend/Pacs/Pacs.Operator.Client/"]
COPY ["Libraries/Core/Backend/Pacs/Pacs.Calls/Pacs.Calls.csproj", "Libraries/Core/Backend/Pacs/Pacs.Calls/"]
COPY ["Libraries/Core/Backend/RevenueService.Client/RevenueService.Client.csproj", "Libraries/Core/Backend/RevenueService.Client/"]
COPY ["Libraries/Infrastructure/Vodovoz.Infrastructure.FileStorage/Vodovoz.Infrastructure.FileStorage.csproj", "Libraries/Infrastructure/Vodovoz.Infrastructure.FileStorage/"]
COPY ["Libraries/Infrastructure/Vodovoz.Infrastructure.S3/Vodovoz.Infrastructure.S3.csproj", "Libraries/Infrastructure/Vodovoz.Infrastructure.S3/"]
COPY ["Libraries/Infrastructure/Vodovoz.Infrastructure.Persistance/Vodovoz.Infrastructure.Persistance.csproj", "Libraries/Infrastructure/Vodovoz.Infrastructure.Persistance/"]
COPY ["Vodovoz.Data.NHibernate/Vodovoz.Data.NHibernate.csproj", "Vodovoz.Data.NHibernate/"]
RUN dotnet restore "Applications/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater"
RUN dotnet build "EdoDocumentFlowUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EdoDocumentFlowUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EdoDocumentFlowUpdater.dll"]
