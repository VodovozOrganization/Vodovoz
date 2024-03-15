using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using QS.Project.Core;

namespace Mango.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(cfg =>
				{
					cfg.UseStartup<Startup>();
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(Vodovoz.Settings.Database.SettingMap).Assembly
							);
				})
				.UseNLog();
	}
}
