using Microsoft.Extensions.DependencyInjection;
using Sms.Internal.Client.Framework;
using Vodovoz.Controllers;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.NotificationRecievers;
using Vodovoz.Services;
using Vodovoz.Settings.Database.Delivery;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Tools.Logistic;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(this IServiceCollection services) => services
			.ConfigureBusinessOptions()
			.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>()
			.AddScoped<IWageParameterService, WageParameterService>()
			.AddScoped<IDeliveryRulesSettings, DeliveryRulesSettings>()
			.AddScoped<IAddressTransferController, AddressTransferController>()
			.AddScoped<IRouteListProfitabilityController, RouteListProfitabilityController>()
			.AddScoped<RouteGeometryCalculator>()
			.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>())
			.AddScoped<IWageCalculationRepository, WageCalculationRepository>()
			.AddScoped<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>()
			.AddScoped<IProfitabilityConstantsRepository, ProfitabilityConstantsRepository>()
			.AddScoped<IRouteListProfitabilityRepository, RouteListProfitabilityRepository>()
			.AddScoped<INomenclatureRepository, NomenclatureRepository>()
			.AddScoped<IFastPaymentSender, FastPaymentSender>()
			.AddScoped<IOrganizationProvider, Stage2OrganizationProvider>()
			.AddScoped<ISmsClientChannelFactory, SmsClientChannelFactory>()
			.AddScoped<ICompletedDriverWarehouseEventRepository, CompletedDriverWarehouseEventRepository>()
			.AddScoped<ICachedDistanceRepository, CachedDistanceRepository>()
			.AddScoped<IGeographicGroupRepository, GeographicGroupRepository>()
			.AddScoped<IDeliveryRepository, DeliveryRepository>()
			.AddScoped<IEmailService, EmailService>()
			.AddScoped<IDeliveryPriceCalculator, DeliveryPriceCalculator>()
			.AddDriverApiHelper();

		public static IServiceCollection ConfigureBusinessOptions(this IServiceCollection services) =>
			services.ConfigureOptions<PushNotificationSettings>();

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
						NotifyOfWaitingTimeChangedURI = databaseSettings.NotifyOfWaitingTimeChangedURI
					};
				})
				.AddScoped<ISmsPaymentStatusNotificationReciever, DriverAPIHelper>()
				.AddScoped<IFastDeliveryOrderAddedNotificationReciever, DriverAPIHelper>()
				.AddScoped<IWaitingTimeChangedNotificationReciever, DriverAPIHelper>()
				.AddScoped<ICashRequestForDriverIsGivenForTakeNotificationReciever, DriverAPIHelper>();
	}
}
