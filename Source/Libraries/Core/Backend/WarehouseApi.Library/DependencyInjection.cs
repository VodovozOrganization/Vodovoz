using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using TrueMarkApi.Client;
using Vodovoz;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Data.NHibernate.Repositories.Employees;
using Vodovoz.FirebaseCloudMessaging;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Errors;
using WarehouseApi.Library.Services;

namespace WarehouseApi.Library
{
	public static class DependencyInjection
	{

		/// <summary>
		/// Добавление сервисов API приложения склада
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddWarehouseApiDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddScoped((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("API приложения склада"))
				.AddCore()
				.AddInfrastructure()
				.AddRepositories()
				.AddTrackedUoW()
				.AddFirebaseCloudMessaging(configuration)
				.ConfigureBusinessOptions(configuration)
				.AddScoped<ICarLoadService, CarLoadService>()
				.AddScoped<ITrueMarkWaterCodeService, TrueMarkWaterCodeService>()
				.AddScoped<IRouteListDailyNumberProvider, RouteListDailyNumberProvider>()
				.AddScoped<CarLoadDocumentConverter>()
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<CarLoadDocumentProcessingErrorsChecker>()
				.AddScoped<TrueMarkApiClientFactory>()
				.AddScoped(sp => sp.GetRequiredService<TrueMarkApiClientFactory>().GetClient())
				.AddScoped<TrueMarkCodesChecker>()
				.AddScoped<ILogisticsEventsCreationService, LogisticsEventsCreationService>()
				.AddScoped<IEmployeeWithLoginRepository, EmployeeWithLoginRepository>();

			services
				.AddMessageTransportSettings()
				.AddEdoMassTransit((context, cfg) =>
				{
					cfg.AddEdoTopology(context);
				});

			services.AddStaticHistoryTracker();

			return services;
		}
	}
}
