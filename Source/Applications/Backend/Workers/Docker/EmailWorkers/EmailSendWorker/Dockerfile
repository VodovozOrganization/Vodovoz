# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/EmailSendWorker.csproj", "Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/"]
COPY ["Libraries/Core/Backend/Email/Mailganer.Api.Client/Mailganer.Api.Client.csproj", "Libraries/Core/Backend/Email/Mailganer.Api.Client/"]
COPY ["Libraries/Core/Backend/Email/RabbitMQ.MailSending/RabbitMQ.EmailSending.csproj", "Libraries/Core/Backend/Email/RabbitMQ.MailSending/"]
COPY ["Libraries/Core/Backend/Infrastructure/MessageTransport/MessageTransport.csproj", "Libraries/Core/Backend/Infrastructure/MessageTransport/"]
COPY ["Libraries/Core/Business/Vodovoz.Settings/Vodovoz.Settings.csproj", "Libraries/Core/Business/Vodovoz.Settings/"]
COPY ["Libraries/Core/Backend/Email/Mailjet.Api.Abstractions/Mailjet.Api.Abstractions.csproj", "Libraries/Core/Backend/Email/Mailjet.Api.Abstractions/"]
COPY ["Libraries/Core/Backend/Infrastructure/ApiClientProvider/ApiClientProvider.csproj", "Libraries/Core/Backend/Infrastructure/ApiClientProvider/"]
COPY ["Libraries/Core/Backend/Infrastructure/RabbitMQ.Infrastructure/RabbitMQ.Infrastructure.csproj", "Libraries/Core/Backend/Infrastructure/RabbitMQ.Infrastructure/"]
RUN dotnet restore "./Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/EmailSendWorker.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker"
RUN dotnet build "./EmailSendWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EmailSendWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailSendWorker.dll"]