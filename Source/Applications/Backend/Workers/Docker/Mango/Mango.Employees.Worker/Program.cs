using Autofac.Extensions.DependencyInjection;
using Mango.Employees.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace Mango.Employees.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly)
						.AddDatabaseConnection()
						.AddCore()
						.AddRepositories()
						.AddTrackedUoW()
						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
						.AddMangoEmployeesServices()
						.ConfigureZabbixSenderFromDataBase(nameof(DriverMangoEmployeeRegistrationWorker))
						.ConfigureZabbixSenderFromDataBase(nameof(DriverMangoEmployeeDeactivationWorker));

					services
						.AddDatabaseConfigurationExposer(config =>
						{
							config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
						});

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

					services.AddHostedService<DriverMangoEmployeeRegistrationWorker>();
					services.AddHostedService<DriverMangoEmployeeDeactivationWorker>();
				});
	}
}
