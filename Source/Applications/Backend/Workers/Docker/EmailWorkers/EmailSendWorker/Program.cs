using System.Net.Http;
using ApiClientProvider;
using Mailjet.Api.Abstractions.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;

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

					services.AddHttpClient();

					services.AddTransient((sp) =>
					{
						var configuration = sp.GetRequiredService<IConfiguration>();
						var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
						var apiHelper = new ApiBasicAuthClientProvider(configuration.GetSection("Mailjet"), httpClient);
						return new SendEndpoint(apiHelper);
					});

					services.AddHostedService<EmailSendWorker>();

				});
	}
}
