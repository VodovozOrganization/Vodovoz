using Microsoft.Extensions.Hosting;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;

namespace CustomerOnlineOrdersUpdater
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
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly,
							typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddTrackedUoW()
						.AddBusiness(hostContext.Configuration)
						.AddInfrastructure()
						.AddConfig(hostContext.Configuration)
						.AddDependenciesGroup()
						.AddApplicationOrderServices()

						.AddStaticScopeForEntity()
						.AddStaticHistoryTracker();
				});
	}
}
