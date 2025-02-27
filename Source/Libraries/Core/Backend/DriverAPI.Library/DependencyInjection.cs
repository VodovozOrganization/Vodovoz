using DriverApi.Notifications.Client.Clients;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Services;
using EventsApi.Library;
using EventsApi.Library.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Controllers;
using Vodovoz.FirebaseCloudMessaging;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.NotificationSenders;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using ITrackPointsServiceV5 = DriverAPI.Library.V5.Services.ITrackPointsService;
using TrackPointsServiceV5 = DriverAPI.Library.V5.Services.TrackPointsService;
using IDriverMobileAppActionRecordServiceV5 = DriverAPI.Library.V5.Services.IDriverMobileAppActionRecordService;
using DriverMobileAppActionRecordServiceV5 = DriverAPI.Library.V5.Services.DriverMobileAppActionRecordService;
using IRouteListServiceV5 = DriverAPI.Library.V5.Services.IRouteListService;
using RouteListServiceV5 = DriverAPI.Library.V5.Services.RouteListService;
using IOrderServiceV5 = DriverAPI.Library.V5.Services.IOrderService;
using OrderServiceV5 = DriverAPI.Library.V5.Services.OrderService;
using IEmployeeServiceV5 = DriverAPI.Library.V5.Services.IEmployeeService;
using EmployeeServiceV5 = DriverAPI.Library.V5.Services.EmployeeService;
using ISmsPaymentServiceV5 = DriverAPI.Library.V5.Services.ISmsPaymentService;
using SmsPaymentServiceV5 = DriverAPI.Library.V5.Services.SmsPaymentService;
using IDriverComplaintServiceV5 = DriverAPI.Library.V5.Services.IDriverComplaintService;
using DriverComplaintServiceV5 = DriverAPI.Library.V5.Services.DriverComplaintService;
using IFastPaymentServiceV5 = DriverAPI.Library.V5.Services.IFastPaymentService;
using FastPaymentServiceV5 = DriverAPI.Library.V5.Services.FastPaymentService;
using VodovozBusiness.Services.TrueMark;
using Vodovoz.Models.TrueMark;
using TrueMarkApi.Client;
using Edo.Transport;
using MessageTransport;
using DriverApi.Notifications.Client;

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
			services.AddVersion6();

			services.AddScoped<IGlobalSettings, GlobalSettings>()
				.AddScoped<ILogisticsEventsService, DriverWarehouseEventsService>();

			services.AddBusiness(configuration)
				.AddDriverApiNotificationsSenders()
				.AddApplication()
				.AddInfrastructure()
				.AddDatabaseSettings()
				.AddDriverEventsDependencies()
				.AddFirebaseCloudMessaging(configuration);

			services
				.AddScoped<IPaymentFromBankClientController, PaymentFromBankClientController>();

			services
				.AddMessageTransportSettings()
				.AddEdoMassTransit();

			return services;
		}

		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			// DAL обертки
			return services.AddScoped<ITrackPointsServiceV5, TrackPointsServiceV5>()
				.AddScoped<IDriverMobileAppActionRecordServiceV5, DriverMobileAppActionRecordServiceV5>()
				.AddScoped<IRouteListServiceV5, RouteListServiceV5>()
				.AddScoped<IOrderServiceV5, OrderServiceV5>()
				.AddScoped<IEmployeeServiceV5, EmployeeServiceV5>()
				.AddScoped<ISmsPaymentServiceV5, SmsPaymentServiceV5>()
				.AddScoped<IDriverComplaintServiceV5, DriverComplaintServiceV5>()
				.AddScoped<IFastPaymentServiceV5, FastPaymentServiceV5>();
		}

		/// <summary>
		/// Добавление сервисов для версии 5
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Обновленная коллекция сервисов</returns>
		public static IServiceCollection AddVersion6(this IServiceCollection services)
		{
			// DAL обертки
			return services.AddScoped<ITrackPointsService, TrackPointsService>()
				.AddScoped<IDriverMobileAppActionRecordService, DriverMobileAppActionRecordService>()
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IOrderService, OrderService>()
				.AddScoped<IEmployeeService, EmployeeService>()
				.AddScoped<ISmsPaymentService, SmsPaymentService>()
				.AddScoped<IDriverComplaintService, DriverComplaintService>()
				.AddScoped<IFastPaymentService, FastPaymentService>()
				.AddTrueMarkCodesCheckDependencies();
		}

		/// <summary>
		/// Добавление сервисов проверки кодов в ЧЗ
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Обновленная коллекция сервисов</returns>
		public static IServiceCollection AddTrueMarkCodesCheckDependencies(this IServiceCollection services)
		{
			return services
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<TrueMarkCodesChecker>()
				.AddScoped<TrueMarkApiClientFactory>()
				.AddScoped(sp => sp.GetRequiredService<TrueMarkApiClientFactory>().GetClient())
				.AddScoped<ITrueMarkWaterCodeService, TrueMarkWaterCodeService>()
				.AddScoped<IRouteListItemTrueMarkProductCodesProcessingService, RouteListItemTrueMarkProductCodesProcessingService>();
		}
	}
}
