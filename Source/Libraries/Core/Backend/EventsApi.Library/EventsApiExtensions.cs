using EventsApi.Library.Models;
using EventsApi.Library.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Data.Interfaces.Logistics.Cars;
using Vodovoz.Core.Data.NHibernate.Repositories.Employees;
using Vodovoz.Core.Data.NHibernate.Repositories.Logistics;
using Vodovoz.Core.Data.NHibernate.Repositories.Logistics.Cars;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.FirebaseCloudMessaging;
using Vodovoz.Settings.Database.Employee;
using Vodovoz.Settings.Employee;
using Vodovoz;

namespace EventsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class EventsApiExtensions
	{
		/// <summary>
		/// Добавление сервисов библиотеки событий
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverEventsDependencies(this IServiceCollection services)
		{
			services.AddLogisticsEventsDependencies()
				.AddScoped<ILogisticsEventsService, DriverWarehouseEventsService>();
			
			return services;
		}
		
		/// <summary>
		/// Добавление сервисов библиотеки событий
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddWarehouseEventsDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddScoped((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Приложение для сканирования событий(склад)"))
				.AddLogisticsEventsDependencies()
				.AddScoped<ILogisticsEventsService, WarehouseEventsService>()
				.AddFirebaseCloudMessaging(configuration)
				.ConfigureBusinessOptions(configuration);

			return services;
		}
		
		/// <summary>
		/// Добавление сервисов библиотеки событий
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		private static IServiceCollection AddLogisticsEventsDependencies(this IServiceCollection services)
		{
			services.AddScoped<ICompletedDriverWarehouseEventProxyRepository, CompletedDriverWarehouseEventProxyRepository>()
				.AddScoped<IEmployeeWithLoginRepository, EmployeeWithLoginRepository>()
				.AddScoped<ICarIdRepository, CarIdRepository>()
				.AddScoped<IDriverWarehouseEventQrDataHandler, DriverWarehouseEventQrDataHandler>()
				.AddScoped<IDriverWarehouseEventSettings, DriverWarehouseEventSettings>();

			return services;
		}
	}
}
