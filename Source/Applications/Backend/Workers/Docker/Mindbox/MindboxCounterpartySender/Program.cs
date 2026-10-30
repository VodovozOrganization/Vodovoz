using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Mindbox.Library;
using Mindbox.Library.Services;
using Vodovoz.Zabbix.Sender;

namespace MindboxCounterpartySender
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
					services.AddLogging(logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
						})

						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly)
						.AddDatabaseConnection()
						.ConfigureZabbixSenderFromDataBase()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW()
						.AddMindbox();

					services.AddScoped<IMindboxAlertSender, ZabbixMindboxAlertSender>();

					services.AddHostedService<MindboxCounterpartySenderWorker>();

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();
				});
		}
	}
}
