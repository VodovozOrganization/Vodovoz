using DriverApi.Notifications.Client;
using Microsoft.Extensions.DependencyInjection;
using RevenueService.Client;
using TrueMarkApi.Client;
using Vodovoz.Application.Clients;
using Vodovoz.Application.Clients.Services;
using Vodovoz.Application.Complaints;
using Vodovoz.Application.Contacts;
using Vodovoz.Application.FileStorage;
using Vodovoz.Application.Goods;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Pacs;
using Vodovoz.Application.Payments;
using Vodovoz.Application.Receipts;
using Vodovoz.Application.Services.Subdivisions;
using Vodovoz.Application.TrueMark;
using Vodovoz.Application.Users;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Service;
using Vodovoz.Handlers;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Models.TrueMark;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Receipts;
using VodovozBusiness.Services.Subdivisions;
using VodovozBusiness.Services.TrueMark;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddSecurityServices()
			.AddApplicationServices()
			.ConfigureFileOptions()
			.AddRevenueServiceClient();

		public static IServiceCollection AddSecurityServices(this IServiceCollection services) => services
			.AddScoped<IUserRoleService, UserRoleService>();

		public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
			.AddSingleton<OperatorService>()
			.AddScoped<ICounterpartyService, CounterpartyService>()
			.AddScoped<IRouteListService, RouteListService>()
			.AddScoped<IRouteListTransferService, RouteListTransferService>()
			.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
			.AddScoped<IPhoneService, PhoneService>()
			.AddScoped<INomenclatureService, NomenclatureService>()
			.AddScoped<IComplaintService, ComplaintService>()
			.AddScoped<ISubdivisionPermissionsService, SubdivisionPermissionsService>()
			.AddScoped<ITrueMarkWaterCodeService, TrueMarkWaterCodeService>()
			.AddScoped<ITrueMarkTransportCodeFactory, TrueMarkTransportCodeFactory>()
			.AddScoped<ITrueMarkWaterGroupCodeFactory, TrueMarkWaterGroupCodeFactory>()
			.AddScoped<ITrueMarkWaterIdentificationCodeFactory, TrueMarkWaterIdentificationCodeFactory>()
			.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>()
			.AddScoped<OurCodesChecker>()
			.AddTrueMarkApiClient()
			.AddApplicationOrderServices()
		;
		
		public static IServiceCollection AddApplicationOrderServices(this IServiceCollection services) => services
			.AddScoped<IOrderService, OrderService>()
			.AddScoped<IPaymentService, PaymentService>()
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
			.AddScoped<IOnlineOrderDiscountHandler, OnlineOrderDiscountHandler>()
			.AddScoped<IOnlineOrderFixedPriceHandler, OnlineOrderFixedPriceHandler>()
			.AddDriverApiNotificationsSenders()
			.AddScoped<IOrderOrganizationManager, OrderOrganizationManager>()
			.AddScoped<IOrderReceiptHandler, OrderReceiptHandler>()
			.AddTransient<IOrganizationForOrderFromSet, OrganizationForOrderFromSet>()
			.AddScoped<IOrganizationForOnlinePaymentService, OrganizationForOnlinePaymentService>()
			.AddTransient<OrderOurOrganizationForOrderHandler>()
			.AddTransient<ContractOrganizationForOrderHandler>()
			.AddTransient<OrganizationByOrderAuthorHandler>()
			.AddTransient<OrganizationByOrderContentForOrderHandler>()
			.AddTransient<OrganizationByPaymentTypeForOrderHandler>()
			.AddTransient<OrganizationForDeliveryOrderByPaymentTypeHandler>()
			.AddTransient<OrganizationForSelfDeliveryOrderByPaymentTypeHandler>()
			.AddTransient<OrganizationFromClientForOrderHandler>()
			.AddScoped<IOrderContractUpdater, OrderContractUpdater>()
			.AddScoped<IOrderConfirmationService, OrderConfirmationService>()
			.AddScoped<IPartitioningOrderService, PartitioningOrderService>()
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
