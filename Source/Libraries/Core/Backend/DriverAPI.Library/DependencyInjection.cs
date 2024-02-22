using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using EventsApi.Library;
using EventsApi.Library.Models;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Common;

namespace DriverAPI.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавление сервисов библиотеки
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverApiLibrary(this IServiceCollection services)
		{
			// Конвертеры
			foreach(var type in typeof(DependencyInjection)
				.Assembly
				.GetTypes()
				.Where(type => type.IsClass)
				.Where(type => type.Name.EndsWith("Converter"))
				.ToList())
			{
				services.AddScoped(type);
			}

			// Хелперы
			services.AddScoped<ISmsPaymentServiceAPIHelper, SmsPaymentServiceAPIHelper>();
			services.AddScoped<IFCMAPIHelper, FCMAPIHelper>();
			services.AddScoped<IActionTimeHelper, ActionTimeHelper>();

			// DAL обертки
			services.AddScoped<ITrackPointsModel, TrackPointsModel>();
			services.AddScoped<IDriverMobileAppActionRecordModel, DriverMobileAppActionRecordModel>();
			services.AddScoped<IRouteListModel, RouteListModel>();
			services.AddScoped<IOrderModel, OrderModel>();
			services.AddScoped<IEmployeeModel, EmployeeModel>();
			services.AddScoped<ISmsPaymentModel, SmsPaymentModel>();
			services.AddScoped<IDriverComplaintModel, DriverComplaintModel>();
			services.AddScoped<IFastPaymentModel, FastPaymentModel>();
			services.AddScoped<ILogisticsEventsService, DriverWarehouseEventsService>();

			services.AddScoped<IGlobalSettings, GlobalSettings>();

			services.AddBusiness();
			services.AddApplication();
			services.AddDatabaseSettings();
			services.AddDriverEventsDependencies();

			return services;
		}
	}
}
