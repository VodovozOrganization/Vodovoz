using DeliveryRulesService.HealthChecks;
using DeliveryRulesService.Workers;
using Fias.Client;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using VodovozHealthCheck;

namespace DeliveryRulesService
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDeliveryRulesService(this IServiceCollection services)
		{
			services
				.AddMvc()
				.AddControllersAsServices();

			services
				.AddControllers()
				.AddJsonOptions(j =>
			{
				//Необходимо для сериализации свойств как PascalCase
				j.JsonSerializerOptions.PropertyNamingPolicy = null;
			});

			services
				.AddMappingAssemblies(
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

				.ConfigureHealthCheckService<DeliveryRulesServiceHealthCheck>()
				.AddHttpClient()
				.AddFiasClient()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			services.AddHostedServices();

			return services;
		}

		public static IServiceCollection AddHostedServices(this IServiceCollection services) =>
			services.AddHostedService<DistrictCacheWorker>();
	}
}
