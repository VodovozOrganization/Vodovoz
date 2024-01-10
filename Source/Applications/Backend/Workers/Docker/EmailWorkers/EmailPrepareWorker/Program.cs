using EmailPrepareWorker.Prepares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;

namespace EmailPrepareWorker
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
					services.AddLogging(loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.AddNLog("NLog.config");
					});

					services.AddTransient<RabbitMQConnectionFactory>();

					services.AddTransient((sp) =>
						sp.GetRequiredService<RabbitMQConnectionFactory>()
							.CreateConnection(sp.GetRequiredService<IConfiguration>()));

					services.AddTransient((sp) =>
					{
						var channel = sp.GetRequiredService<IConnection>().CreateModel();
						channel.BasicQos(0, 1, false);
						return channel;
					});

					services.AddCore();
					services.AddTrackedUoW();

					services.AddSingleton<ISettingsController, SettingsController>();
					services.AddSingleton<IEmailParametersProvider, EmailParametersProvider>();
					services.AddSingleton<IEmailRepository, EmailRepository>();
					services.AddSingleton<IEmailDocumentPreparer, EmailDocumentPreparer>();
					services.AddSingleton<IEmailSendMessagePreparer, EmailSendMessagePreparer>();

					services.AddHostedService<EmailPrepareWorker>();
				});
	}
}
