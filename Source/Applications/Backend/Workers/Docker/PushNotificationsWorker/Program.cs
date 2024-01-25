using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;

namespace PushNotificationsWorker
{
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((hostContext, loggingBuilder) =>
				{
					loggingBuilder.ClearProviders();
					loggingBuilder.AddNLogWeb();
					loggingBuilder.AddConfiguration(hostContext.Configuration.GetSection(_nLogSectionName));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddTrackedUoW()
						.AddServiceUser()
						.AddPushNotificationsWorker(hostContext);

					services.AddStaticHistoryTracker();
				})
			.UseWindowsService();
	}
}
