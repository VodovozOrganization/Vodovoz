using DeliveryRulesService.Cache;
using DeliveryRulesService.HealthChecks;
using DeliveryRulesService.Workers;
using Fias.Client;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using QS.Services;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using VodovozHealthCheck;

namespace DeliveryRulesService
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDeliveryRulesService(this IServiceCollection services)
		{
			services
				.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeliveryRulesService", Version = "v1" });
				})
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

			services
				.AddSingleton<DistrictCacheService>()
				.AddScoped<IErrorReporter, ErrorReporter>(_ => ErrorReporter.Instance)
				.AddScoped<IUserService, UserService>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<IFastDeliveryAvailabilityHistoryModel, FastDeliveryAvailabilityHistoryModel>()
				.AddInfrastructure()
				.AddHostedServices();

			return services;
		}

		public static IServiceCollection AddHandlers(this IServiceCollection services)
		{
			var typesToRegister = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.Name.EndsWith("Handler"));

			return services;
		}

		public static IServiceCollection AddHostedServices(this IServiceCollection services) =>
			services.AddHostedService<DistrictCacheWorker>();
	}
}
