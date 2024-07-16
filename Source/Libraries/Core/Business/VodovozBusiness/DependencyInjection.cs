using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sms.Internal.Client.Framework;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Common;
using Vodovoz.EntityRepositories;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.NotificationRecievers;
using Vodovoz.Options;
using Vodovoz.Services;
using Vodovoz.Settings.Database.Delivery;
using Vodovoz.Settings.Database.Fuel;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Fuel;
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

		private static IServiceCollection RegisterClassesByInterfaces(
			this IServiceCollection services, string classEndsWith, DependencyType dependencyType = DependencyType.Scoped)
		{
			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith(classEndsWith)
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				switch(dependencyType)
				{
					case DependencyType.Singleton:
						services.AddSingleton(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
					case DependencyType.Scoped:
						services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
					case DependencyType.Transient:
						services.AddTransient(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
				}
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

	public enum DependencyType
	{
		Singleton,
		Scoped,
		Transient
	}
}
