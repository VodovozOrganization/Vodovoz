using System;
using System.Net.Http;
using System.Net.Security;
using ApiClientProvider;
using EmailSendWorker.Consumers;
using Mailjet.Api.Abstractions.Configs;
using Mailjet.Api.Abstractions.Endpoints;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using Vodovoz.Settings.Pacs;

namespace EmailSendWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					})

					.AddConfig(hostContext.Configuration)
					.AddTransient<RabbitMQConnectionFactory>()

					.AddTransient(sp =>
					{
						var messageTransportSettings = sp.GetRequiredService<IMessageTransportSettings>();
						Enum.TryParse<SslPolicyErrors>(messageTransportSettings.AllowSslPolicyErrors, out var sslPolicyErrors);
						return sp.GetRequiredService<RabbitMQConnectionFactory>()
							.CreateConnection(
								messageTransportSettings.Host,
								messageTransportSettings.Username,
								messageTransportSettings.Password,
								messageTransportSettings.VirtualHost,
								messageTransportSettings.Port,
								messageTransportSettings.UseSSL,
								sslPolicyErrors
							);
					})

					.AddTransient(sp =>
					{
						var channel = sp.GetRequiredService<IConnection>().CreateModel();
						channel.BasicQos(0, 1, false);
						return channel;
					})

					.AddHttpClient()
					.AddTransient((sp) =>
					{
						var configuration = sp.GetRequiredService<IConfiguration>();
						var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
						var apiHelper = new ApiBasicAuthClientProvider(configuration.GetSection(MailjetOptions.Path), httpClient);
						return new SendEndpoint(apiHelper);
					})
					
					.Configure<MailjetOptions>(hostContext.Configuration.GetSection(MailjetOptions.Path))
					.AddMassTransit(busConf =>
					{
						busConf.AddConsumer<EmailSendConsumer, EmailSendConsumerDefinition>();
						busConf.ConfigureRabbitMq();
					})
					
					.AddHostedService<EmailSendWorker>();
				});
	}
}
