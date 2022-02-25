#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Workers/EmailSendWorker/EmailSendWorker.csproj", "Services/Workers/EmailSendWorker/"]
COPY ["Services/Library/RabbitMQ.Infrastructure/RabbitMQ.Infrastructure.csproj", "Services/Library/RabbitMQ.Infrastructure/"]
COPY ["Services/Library/RabbitMQ.MailSending/RabbitMQ.EmailSending.csproj", "Services/Library/RabbitMQ.MailSending/"]
COPY ["Services/Library/Mailjet.Api.Abstractions/Mailjet.Api.Abstractions.csproj", "Services/Library/Mailjet.Api.Abstractions/"]
COPY ["Services/Library/ApiClientProvider/ApiClientProvider.csproj", "Services/Library/ApiClientProvider/"]
RUN dotnet restore "Services/Workers/EmailSendWorker/EmailSendWorker.csproj"
COPY . .
WORKDIR "/src/Services/Workers/EmailSendWorker"
RUN dotnet build "EmailSendWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailSendWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailSendWorker.dll"]