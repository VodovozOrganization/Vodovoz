using Autofac.Extensions.DependencyInjection;
using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EdoManualEventSender
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			Console.InputEncoding = System.Text.Encoding.UTF8;

			ServiceCollection services = new ServiceCollection();

			var projectSettingsPath = Path.Combine(GetProjectDirectory(), "appsettings.Development.json");
			var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json");

			if(File.Exists(projectSettingsPath))
			{
				settingsPath = projectSettingsPath;
			}

			var builder = new ConfigurationBuilder();
			builder.SetBasePath(GetProjectDirectory());
			builder.AddJsonFile(settingsPath);
			IConfiguration configuration = builder.Build();
			services.AddScoped<IConfiguration>(_ => configuration);

			services.AddLogging(loggingBuilder =>
			{
				// configure Logging with NLog
				loggingBuilder.ClearProviders();
				loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
				loggingBuilder.AddNLog(configuration);
			});

			services.AddMessageTransportSettings();
			services.AddEdoMassTransit();
			services.AddScoped<EdoEventsSender>();

			var autofacFactory = new AutofacServiceProviderFactory();
			var autofacBuilder = autofacFactory.CreateBuilder(services);
			var serviceProvider = autofacFactory.CreateServiceProvider(autofacBuilder);

			var eventsSender = serviceProvider.GetRequiredService<EdoEventsSender>();

			while(true)
			{
				try
				{
					Task.Delay(1000).Wait();
					eventsSender.TrySendEdoEvent();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		private static string GetProjectDirectory()
		{
			string current = AppContext.BaseDirectory;
			while(!Directory.GetFiles(current, "*.csproj").Any() && Directory.GetParent(current) != null)
			{
				current = Directory.GetParent(current)?.FullName ?? current;
			}
			return current;
		}
	}
}
