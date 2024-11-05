using DriverAPI.Library.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using EventsApi.Library;
using EventsApi.Library.Models;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Common;
using Vodovoz.Controllers;
using DriverAPI.Library.V5.Services;
using Vodovoz.FirebaseCloudMessaging;
using Microsoft.Extensions.Configuration;
using Vodovoz.NotificationRecievers;

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
		public static IServiceCollection AddDriverApiLibrary(this IServiceCollection services, IConfiguration configuration)
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
			services.AddScoped<ISmsPaymentServiceAPIHelper, SmsPaymentServiceAPIHelper>()
				.AddScoped<IActionTimeHelper, ActionTimeHelper>();

			services.AddVersion5();

			services.AddScoped<IGlobalSettings, GlobalSettings>()
				.AddScoped<ILogisticsEventsService, DriverWarehouseEventsService>();

			services.AddBusiness(configuration)
				.AddApplication()
				.AddDatabaseSettings()
				.AddDriverEventsDependencies()
				.AddFirebaseCloudMessaging(configuration);

			services
				.AddScoped<IPaymentFromBankClientController, PaymentFromBankClientController>()
				.AddScoped<IRouteListTransferhandByHandReciever, DriverAPIHelper>();

			return services;
		}

		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			// DAL обертки
			return services.AddScoped<ITrackPointsService, TrackPointsService>()
				.AddScoped<IDriverMobileAppActionRecordService, DriverMobileAppActionRecordService>()
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IOrderService, OrderService>()
				.AddScoped<IEmployeeService, EmployeeService>()
				.AddScoped<ISmsPaymentService, SmsPaymentService>()
				.AddScoped<IDriverComplaintService, DriverComplaintService>()
				.AddScoped<IFastPaymentService, FastPaymentService>();
		}
	}
}
