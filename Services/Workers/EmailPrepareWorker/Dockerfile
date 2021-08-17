#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0.8-focal AS base
RUN apt update
RUN apt install libgdiplus -y
RUN apt install fonts-dejavu-extra -y
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Workers/DocumentForEmailPreparingWorker/DocumentForEmailPreparingWorker.csproj", "Services/Workers/DocumentForEmailPreparingWorker/"]
COPY ["../QSProjects/QSProjectsLib/QSProjectsLib.csproj", "../QSProjects/QSProjectsLib/"]
COPY ["../QSProjects/QS.MachineConfig/QS.MachineConfig.csproj", "../QSProjects/QS.MachineConfig/"]
COPY ["../QSProjects/QSSaaS/QSSaaS.csproj", "../QSProjects/QSSaaS/"]
COPY ["../QSProjects/QS.Project/QS.Project.csproj", "../QSProjects/QS.Project/"]
COPY ["../QSProjects/QS.Project.Core/QS.Project.Core.csproj", "../QSProjects/QS.Project.Core/"]
COPY ["../QSProjects/QS.Utilities/QS.Utilities.csproj", "../QSProjects/QS.Utilities/"]
COPY ["../Gtk.DataBindings/System.Data.Bindings/System.Data.Bindings.csproj", "../Gtk.DataBindings/System.Data.Bindings/"]
COPY ["../GammaBinding/Gamma.Utilities/Gamma.Utilities.csproj", "../GammaBinding/Gamma.Utilities/"]
COPY ["../QSProjects/Modules/QS.Banks/QS.Banks.csproj", "../QSProjects/Modules/QS.Banks/"]
COPY ["../QSProjects/QS.Report/QS.Report.csproj", "../QSProjects/QS.Report/"]
COPY ["../My-FyiReporting/RdlEngine/RdlEngine.csproj", "../My-FyiReporting/RdlEngine/"]
COPY ["../My-FyiReporting/DataProviders/DataProviders.csproj", "../My-FyiReporting/DataProviders/"]
COPY ["Services/Library/RabbitMQ.Infrastructure/RabbitMQ.Infrastructure.csproj", "Services/Library/RabbitMQ.Infrastructure/"]
COPY ["VodovozBusiness/VodovozBusiness.csproj", "VodovozBusiness/"]
COPY ["../QSProjects/Modules/QS.BusinessCommon/QS.BusinessCommon.csproj", "../QSProjects/Modules/QS.BusinessCommon/"]
COPY ["../QSProjects/QS.DocTemplates/QS.DocTemplates.csproj", "../QSProjects/QS.DocTemplates/"]
COPY ["../QSProjects/QS.Osm/QS.Osm.csproj", "../QSProjects/QS.Osm/"]
COPY ["../GMap.NET/GMap.NET.Core/GMap.NET.Core.csproj", "../GMap.NET/GMap.NET.Core/"]
COPY ["../QSProjects/QS.HistoryLog/QS.HistoryLog.csproj", "../QSProjects/QS.HistoryLog/"]
COPY ["VodovozInfrastructure/VodovozInfrastructure.csproj", "VodovozInfrastructure/"]
COPY ["Services/Library/RabbitMQ.EmailSending/RabbitMQ.MailSending.csproj", "Services/Library/RabbitMQ.EmailSending/"]
COPY ["Services/Library/Mailjet.Api.Abstractions/Mailjet.Api.Abstractions.csproj", "Services/Library/Mailjet.Api.Abstractions/"]
COPY ["Services/Library/ApiHelper/ApiHelper.csproj", "Services/Library/ApiHelper/"]
RUN dotnet restore "Services/Workers/DocumentForEmailPreparingWorker/DocumentForEmailPreparingWorker.csproj"
COPY . .
WORKDIR "/src/Services/Workers/DocumentForEmailPreparingWorker"
RUN dotnet build "DocumentForEmailPreparingWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentForEmailPreparingWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocumentForEmailPreparingWorker.dll"]