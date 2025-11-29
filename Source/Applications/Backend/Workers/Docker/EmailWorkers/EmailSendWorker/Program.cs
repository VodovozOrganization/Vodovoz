using Autofac.Extensions.DependencyInjection;
using EmailSendWorker.Consumers;
using EmailSendWorker.Factoies;
using EmailSendWorker.Services;
using Mailganer.Api.Client;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Extensions.Logging;
using QS.Project.Core;
using RabbitMQ.Client;
using RabbitMQ.EmailSending.Masstransit;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Net.Security;
using Vodovoz.Core.Data.NHibernate;

namespace EmailSendWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					});

					services.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
					);

					services
						.AddDatabaseConnection()
						.AddCore()
						.AddNotTrackedUoW();

					services
						.AddHttpClient()
						.AddMailganerApiClient();

					services
						.AddTransient<IEmailSendService, EmailSendService>()
						.AddTransient<IEmailMessageFactory, EmailMessageFactory>();

					services
						.AddMassTransit(busConf =>
						{
							var transportSettings = new ConfigTransportSettings();
							hostContext.Configuration.Bind("MessageBroker", transportSettings);

							busConf.AddConsumer<AuthorizationCodesEmailSendConsumer, AuthorizationCodesEmailSendConsumerDefinition>();
							busConf.AddConsumer<SendEmailMessageConsumer, SendEmailMessageConsumerDefinition>();
							busConf.ConfigureRabbitMq((rabbitMq, context) =>
							{
								rabbitMq.AddSendEmailMessageTopology(context);
								rabbitMq.AddSendAuthorizationCodesByEmailTopology(context);
								rabbitMq.AddUpdateEmailStatusTopology(context);
							},
							transportSettings);
						})
						.AddMassTransit<IEmailSendBus>(busConf =>
						{
							busConf.AddConsumer<SendEmailMessageConsumer, SendEmailMessageConsumerDefinition>();
							busConf.ConfigureRabbitMq((rabbitMq, context) =>
							{
								rabbitMq.AddSendEmailMessageTopology(context);
							});
						});

					services
						.AddTransient<RabbitMQConnectionFactory>()
						.AddTransient(sp =>
						{
							var messageTransportSettings = new ConfigTransportSettings();
							hostContext.Configuration.Bind("MessageBroker", messageTransportSettings);
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
						});

					services.AddHostedService<EmailSendWorker>();
				});
	}
}
