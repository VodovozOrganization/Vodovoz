using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.Utilities.Extensions;
using Sms.Internal.Client.Framework;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.NotificationRecievers;
using Vodovoz.Options;
using Vodovoz.Settings.Logistics;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.Tools.Orders;
using Vodovoz.Validation;
using Vodovoz.Infrastructure.Persistance;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(
			this IServiceCollection services,
			IConfiguration configuration,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) =>
			services
				.RegisterClassesByInterfaces("Controller", serviceLifetime)
				.RegisterClassesByInterfaces("Converter", serviceLifetime)
				.RegisterClassesByInterfaces("Repository", serviceLifetime)
				.RegisterClassesByInterfaces("Service", serviceLifetime)
				.RegisterClassesByInterfaces("Handler", serviceLifetime)
				.RegisterClassesByInterfaces("Factory", serviceLifetime)
				
				.ConfigureBusinessOptions(configuration)
				.AddService<RouteGeometryCalculator>(serviceLifetime)
				.AddService<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>(), serviceLifetime)
				.AddService<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>(serviceLifetime)
				.AddService<IFastPaymentSender, FastPaymentSender>(serviceLifetime)
				.AddService<IOrganizationProvider, Stage2OrganizationProvider>(serviceLifetime)
				.AddService<ISmsClientChannelFactory, SmsClientChannelFactory>(serviceLifetime)
				.AddService<IDeliveryPriceCalculator, DeliveryPriceCalculator>(serviceLifetime)
				.AddService<FastDeliveryHandler>(serviceLifetime)
				.AddService<IFastDeliveryValidator, FastDeliveryValidator>(serviceLifetime)
				.AddService<ICallTaskWorker, CallTaskWorker>(serviceLifetime)
				.AddService<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance(), serviceLifetime)
				.AddService<IErrorReporter>(context => ErrorReporter.Instance, serviceLifetime)
				.AddService<OrderStateKey>(serviceLifetime)
				.AddService<OnlineOrderStateKey>(serviceLifetime)
				.AddDriverApiHelper()
			;

		private static IServiceCollection RegisterClassesByInterfaces(
			this IServiceCollection services,
			string classEndsWith,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			services.AddServicesEndsWith(typeof(DependencyInjection).Assembly, classEndsWith, serviceLifetime);
			return services;
		}

		public static IServiceCollection ConfigureBusinessOptions(this IServiceCollection services, IConfiguration configuration) => services
			.Configure<PushNotificationSettings>(pushNotificationOptions =>
				configuration.GetSection(nameof(PushNotificationSettings)).Bind(pushNotificationOptions));

		public static IServiceCollection AddDriverApiHelper(this IServiceCollection services) =>
			services.AddScoped<DriverApiHelperConfiguration>(serviceProvider =>
				{
					var databaseSettings = serviceProvider.GetRequiredService<IDriverApiSettings>();
					return new DriverApiHelperConfiguration
					{
						ApiBase = databaseSettings.ApiBase,
						NotifyOfCashRequestForDriverIsGivenForTakeUri = databaseSettings.NotifyOfCashRequestForDriverIsGivenForTakeUri,
						NotifyOfFastDeliveryOrderAddedURI = databaseSettings.NotifyOfFastDeliveryOrderAddedUri,
						NotifyOfSmsPaymentStatusChangedURI = databaseSettings.NotifyOfSmsPaymentStatusChangedUri,
						NotifyOfWaitingTimeChangedURI = databaseSettings.NotifyOfWaitingTimeChangedURI,
						NotifyOfOrderWithGoodsTransferingIsTransferedUri = databaseSettings.NotifyOfOrderWithGoodsTransferingIsTransferedUri,
					};
				})
				.AddScoped<ISmsPaymentStatusNotificationReciever, DriverAPIHelper>()
				.AddScoped<IFastDeliveryOrderAddedNotificationReciever, DriverAPIHelper>()
				.AddScoped<IWaitingTimeChangedNotificationReciever, DriverAPIHelper>()
				.AddScoped<ICashRequestForDriverIsGivenForTakeNotificationReciever, DriverAPIHelper>()
				.AddScoped<IRouteListTransferhandByHandReciever, DriverAPIHelper>();
	}
}
