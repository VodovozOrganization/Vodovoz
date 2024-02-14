using Autofac.Extensions.DependencyInjection;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Pacs.MangoCalls;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Settings.Database;

namespace Pacs.Calls.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddLogging(logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
						})
						.AddMappingAssemblies(
							typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(Vodovoz.Settings.Database.SettingMap).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddTrackedUoW()

						//Настройки бд должны регистрироваться до настроек MassTransit
						.AddDatabaseSettings()

						.AddMessageTransportSettings()
						.AddPacsMangoCallsServices()
						;
				});
		}
	}
}
