using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using RobotMiaApi.Services;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Settings.Database;
using Vodovoz;

namespace RobotMiaApi
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddRobotMiaApi(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddBusiness(configuration)
				.AddInfrastructure()
				.AddApplication()
				.AddTrackedUoW()
				.AddDatabaseSettings()
				.AddScoped<IUnitOfWork>((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddScoped<IncomingCallCallService>()
				.AddScoped<OrderService>()
				.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services
				.AddSecurity(configuration)
				.AddVersioning()
				.AddFeatureFlags();

			services.AddControllers()
				.AddSharedControllers();

			return services;
		}
	}
}
