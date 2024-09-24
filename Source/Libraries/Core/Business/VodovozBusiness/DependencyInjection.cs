using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.Utilities.Extensions;
using Sms.Internal.Client.Framework;
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

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(
			this IServiceCollection services,
			IConfiguration configuration,
			DependencyType dependencyType = DependencyType.Scoped) =>
			services
				.RegisterClassesByInterfaces("Controller", dependencyType)
				.RegisterClassesByInterfaces("Converter", dependencyType)
				.RegisterClassesByInterfaces("Repository", dependencyType)
				.RegisterClassesByInterfaces("Service", dependencyType)
				.RegisterClassesByInterfaces("Handler", dependencyType)
				.RegisterClassesByInterfaces("Factory", dependencyType)
				
				.ConfigureBusinessOptions(configuration)
				.AddService<RouteGeometryCalculator>(dependencyType)
				.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>())
				.AddService<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>(dependencyType)
				.AddService<IFastPaymentSender, FastPaymentSender>(dependencyType)
				.AddService<IOrganizationProvider, Stage2OrganizationProvider>(dependencyType)
				.AddService<ISmsClientChannelFactory, SmsClientChannelFactory>(dependencyType)
				.AddService<IDeliveryPriceCalculator, DeliveryPriceCalculator>(dependencyType)
				.AddService<FastDeliveryHandler>(dependencyType)
				.AddService<IFastDeliveryValidator, FastDeliveryValidator>(dependencyType)
				.AddService<ICallTaskWorker, CallTaskWorker>(dependencyType)
				.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
				.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
				.AddService<OrderStateKey>(dependencyType)
				.AddService<OnlineOrderStateKey>(dependencyType)
				.AddDriverApiHelper()
			;

		private static IServiceCollection RegisterClassesByInterfaces(
			this IServiceCollection services,
			string classEndsWith,
			DependencyType dependencyType = DependencyType.Scoped)
		{
			services.AddServicesEndsWith(typeof(DependencyInjection).Assembly, classEndsWith, dependencyType);
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
