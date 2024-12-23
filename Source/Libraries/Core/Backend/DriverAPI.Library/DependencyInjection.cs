using DriverAPI.Library.Helpers;
using DriverAPI.Library.V5.Services;
using EventsApi.Library;
using EventsApi.Library.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FirebaseCloudMessaging;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.NotificationRecievers;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;

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
				.AddInfrastructure()
				.AddDatabaseSettings()
				.AddDriverEventsDependencies()
				.AddFirebaseCloudMessaging(configuration);

			services
				.AddScoped<IPaymentFromBankClientController, PaymentFromBankClientController>()
				.AddScoped<IRouteListTransferReciever, DriverAPIHelper>();

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
