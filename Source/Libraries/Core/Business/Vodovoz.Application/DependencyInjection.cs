using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Contacts;
using Vodovoz.Application.Complaints;
using Vodovoz.Application.FileStorage;
using Vodovoz.Application.Goods;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Pacs;
using Vodovoz.Application.Payments;
using Vodovoz.Application.Services;
using Vodovoz.Application.Services.Subdivisions;
using Vodovoz.Domain.Service;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Subdivisions;
using DriverApi.Notifications.Client;
using Vodovoz.Application.Receipts;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddScoped<IRouteOptimizer, RouteOptimizer>()
			.AddApplicationServices()
			.ConfigureFileOptions();

		public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
			.AddSingleton<OperatorService>()
			.AddScoped<ICounterpartyService, CounterpartyService>()
			.AddScoped<IRouteListService, RouteListService>()
			.AddScoped<IPaymentService, PaymentService>()
			.AddScoped<IOrderService, OrderService>()
			.AddScoped<IPhoneService, PhoneService>()
			.AddScoped<INomenclatureService, NomenclatureService>()
			.AddScoped<IComplaintService, ComplaintService>()
			.AddScoped<ISubdivisionPermissionsService, SubdivisionPermissionsService>()
			.AddOrderServicesDependencies()
		;
		
		public static IServiceCollection AddApplicationOrderServices(this IServiceCollection services) => services
			.AddScoped<IOrderService, OrderService>()
			.AddOrderServicesDependencies()
			;

		private static IServiceCollection AddOrderServicesDependencies(this IServiceCollection services) => services
			.AddScoped<IOnlineOrderDeliveryPriceGetter, OnlineOrderDeliveryPriceGetter>()
			.AddScoped<IOrderFromOnlineOrderCreator, OrderFromOnlineOrderCreator>()
			.AddScoped<IOrderFromOnlineOrderValidator, OrderFromOnlineOrderValidator>()
			.AddScoped<IGoodsPriceCalculator, GoodsPriceCalculator>()
			.AddScoped<IOrderDeliveryPriceGetter, OrderDeliveryPriceGetter>()
			.AddScoped<IClientDeliveryPointsChecker, ClientDeliveryPointsChecker>()
			.AddScoped<IFreeLoaderChecker, FreeLoaderChecker>()
			.AddDriverApiNotificationsSenders()
			.AddScoped<IOrderOrganizationManager, OrderOrganizationManager>()
			.AddScoped<IOrderReceiptHandler, OrderReceiptHandler>()
			.AddTransient<IOrganizationForOrderFromSet, OrganizationForOrderFromSet>()
			.AddTransient<OrderOurOrganizationForOrderHandler>()
			.AddTransient<ContractOrganizationForOrderHandler>()
			.AddTransient<OrganizationByOrderAuthorHandler>()
			.AddTransient<OrganizationByOrderContentForOrderHandler>()
			.AddTransient<OrganizationByPaymentTypeForOrderHandler>()
			.AddTransient<OrganizationForDeliveryOrderByPaymentTypeHandler>()
			.AddTransient<OrganizationForSelfDeliveryOrderByPaymentTypeHandler>()
			.AddTransient<OrganizationFromClientForOrderHandler>()
			.AddScoped<IOrderContractUpdater, OrderContractUpdater>()
		;

		private static IServiceCollection ConfigureFileOptions(this IServiceCollection services)
			=> services.Configure<FileSecurityOptions>(options =>
			{
				options.RestrictedToOpenExtensions = new string[]
				{
					".exe",
					".cmd",
					".bat",
					".jar",
					".ps1"
				};
			});
	}
}
