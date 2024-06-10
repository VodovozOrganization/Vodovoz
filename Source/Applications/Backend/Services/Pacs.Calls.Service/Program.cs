using Autofac.Extensions.DependencyInjection;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Pacs.MangoCalls;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;

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
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddDatabaseConnection()
						.AddCore()
						.AddTrackedUoW()

						.AddMessageTransportSettings()
						.AddPacsMangoCallsServices()
						;
				});
		}
	}
}
