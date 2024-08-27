using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sms.Internal.Client.Framework;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;
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
		public static IServiceCollection AddBusiness(this IServiceCollection services, IConfiguration configuration) => services

			.RegisterClassesByInterfaces("Controller")
			.RegisterClassesByInterfaces("Repository")
			.RegisterClassesByInterfaces("Service")
			.RegisterClassesByInterfaces("Handler")
			.RegisterClassesByInterfaces("Factory")
			
			.ConfigureBusinessOptions(configuration)
			.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
			.AddScoped<RouteGeometryCalculator>()
			.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>())
			.AddScoped<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>()
			.AddScoped<IFastPaymentSender, FastPaymentSender>()
			.AddScoped<IOrganizationProvider, Stage2OrganizationProvider>()
			.AddScoped<ISmsClientChannelFactory, SmsClientChannelFactory>()
			.AddScoped<IDeliveryPriceCalculator, DeliveryPriceCalculator>()
			.AddScoped<FastDeliveryHandler>()
			.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
			.AddScoped<ICallTaskWorker, CallTaskWorker>()
			.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
			.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
			.AddScoped<OrderStateKey>()
			.AddScoped<OnlineOrderStateKey>()
			.AddDriverApiHelper()
		;

		private static IServiceCollection RegisterClassesByInterfaces(this IServiceCollection services, string classEndsWith)
		{
			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith(classEndsWith)
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
			}
			
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
