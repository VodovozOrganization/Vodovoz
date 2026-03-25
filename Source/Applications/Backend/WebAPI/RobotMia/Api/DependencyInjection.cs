using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Trackers;

namespace Vodovoz.RobotMia.Api
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddRobotMiaApi(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(HistoryMain).Assembly,
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
				.AddOrderTrackerFor1c()
				.AddDatabaseSettings()
				.AddScoped((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddScoped<INomenclatureService, NomenclatureService>()
				.AddScoped<IIncomingCallCallService, IncomingCallCallService>()
				.AddScoped<IOrderService, OrderService>()
				.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services
				.AddSecurity(configuration)
				.AddAuthorizationIfNeeded();

			services.AddControllers()
				.AddSharedControllers();

			services
				.AddVersioning();

			return services;
		}
	}
}
