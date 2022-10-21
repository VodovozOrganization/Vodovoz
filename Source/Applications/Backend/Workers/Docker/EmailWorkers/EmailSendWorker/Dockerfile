#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/EmailSendWorker.csproj", "Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/"]
COPY ["Libraries/Core/Backend/Email/RabbitMQ.MailSending/RabbitMQ.EmailSending.csproj", "Libraries/Core/Backend/Email/RabbitMQ.MailSending/"]
COPY ["Libraries/Core/Backend/Email/Mailjet.Api.Abstractions/Mailjet.Api.Abstractions.csproj", "Libraries/Core/Backend/Email/Mailjet.Api.Abstractions/"]
COPY ["Libraries/Core/Backend/Infrastructure/ApiClientProvider/ApiClientProvider.csproj", "Libraries/Core/Backend/Infrastructure/ApiClientProvider/"]
COPY ["Libraries/Core/Backend/Infrastructure/RabbitMQ.Infrastructure/RabbitMQ.Infrastructure.csproj", "Libraries/Core/Backend/Infrastructure/RabbitMQ.Infrastructure/"]
RUN dotnet restore "Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/EmailSendWorker.csproj"
COPY . .
WORKDIR "/src/Applications/Backend/Workers/Docker/EmailWorkers/EmailSendWorker"
RUN dotnet build "EmailSendWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailSendWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailSendWorker.dll"]
