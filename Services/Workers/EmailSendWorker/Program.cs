using ApiClientProvider;
using Mailjet.Api.Abstractions.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using Vodovoz.EntityRepositories;

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

					services.AddTransient((sp) =>
					{
						var configuration = sp.GetRequiredService<IConfiguration>();
						var apiHelper = new ApiBasicAuthClientProvider(configuration.GetSection("Mailjet"));
						return new SendEndpoint(apiHelper);
					});

					services.AddHostedService<EmailSendWorker>();

					services.AddTransient<IEmailRepository, EmailRepository>();
				});
	}
}
