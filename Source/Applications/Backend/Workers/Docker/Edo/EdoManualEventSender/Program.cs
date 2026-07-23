using Autofac.Extensions.DependencyInjection;
using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
			var hostBuilder = new HostBuilder();

			hostBuilder
				.ConfigureLogging((context, loggingBuilder) =>
				{
					// configure Logging with NLog
					loggingBuilder.ClearProviders();
					loggingBuilder.SetMinimumLevel(LogLevel.Trace);
					loggingBuilder.AddNLog(context.Configuration);
				})
				.ConfigureServices(services =>
				{
					services.AddMessageTransportSettings();
					services.AddEdoMassTransit();
					services.AddScoped<EdoEventsSender>();
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory());

			var app = hostBuilder.Build();

			Console.OutputEncoding = System.Text.Encoding.UTF8;
			Console.InputEncoding = System.Text.Encoding.UTF8;

			var eventsSender = app.Services.GetRequiredService<EdoEventsSender>();

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
