using DriverApi.Notifications.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RevenueService.Client;
using Sms.Internal.Client;
using TrueMarkApi.Client;
using Vodovoz.Core.Application.Clients;
using Vodovoz.Core.Application.Clients.Services;
using Vodovoz.Core.Application.Complaints;
using Vodovoz.Core.Application.Contacts;
using Vodovoz.Core.Application.Employees;
using Vodovoz.Core.Application.FastPayment;
using Vodovoz.Core.Application.FileStorage;
using Vodovoz.Core.Application.Goods;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Application.Orders.Services.OrderCancellation;
using Vodovoz.Core.Application.Payments;
using Vodovoz.Core.Application.Receipts;
using Vodovoz.Core.Application.Services.Subdivisions;
using Vodovoz.Core.Application.TrueMark;
using Vodovoz.Core.Application.Users;
using Vodovoz.Core.Application.Warehouses;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Service;
using Vodovoz.Handlers;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Employees;
using VodovozBusiness.Models.TrueMark;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Clients;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Receipts;
using VodovozBusiness.Services.Subdivisions;
using VodovozBusiness.Services.TrueMark;

namespace Vodovoz.Core.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCoreApplication(this IServiceCollection services) => services
			.AddCoreSecurityServices()
			.AddCoreApplicationServices()
			.ConfigureFileOptions()
			.AddRevenueServiceClient();

		public static IServiceCollection AddCoreSecurityServices(this IServiceCollection services) => services
			.AddScoped<IUserRoleService, UserRoleService>()
			.AddScoped<GrantsRoleParser>();

		public static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
		{
			services
				.AddScoped<ICounterpartyService, CounterpartyService>()
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListTransferService, RouteListTransferService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
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
				.AddScoped<IWarehousePermissionService, WarehousePermissionService>()
				.AddScoped<IExternalApplicationUserService, ExternalApplicationUserService>()
				.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>()
				.AddScoped<OurCodesChecker>()
				.AddScoped<OrderCancellationService>()
				.AddScoped<SelfdeliveryCancellationService>()
				.AddScoped<IExternalCounterpartyHandler, ExternalCounterpartyHandler>()
				.AddScoped<IStagingTrueMarkCodeFactory, StagingTrueMarkCodeFactory>()
				.AddTrueMarkApiClient()
				.AddCoreApplicationOrderServices()				
				;

			services.TryAddScoped<IFastPaymentSender, FastPaymentSender>();
			services.TryAddScoped<SmsClientChannelFactory>();

			return services;
		}

		public static IServiceCollection AddCoreApplicationOrderServices(this IServiceCollection services) => services
			.AddScoped<IOrderService, OrderService>()
			.AddScoped<IPaymentService, PaymentService>()
			.AddCoreOrderServicesDependencies()
			.AddScoped<IClosingDeliveriesService, ClosingDeliveriesService>()
			;

		private static IServiceCollection AddCoreOrderServicesDependencies(this IServiceCollection services) => services
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
			.AddScoped<IUnPaidOnlineOrderHandler, UnPaidOnlineOrderHandler>()
			.AddScoped<ICustomerOrderTransferService, CustomerOrderTransferService>()
			.AddScoped<IOrderOnlinePaymentAcceptanceHandler, OrderOnlinePaymentAcceptanceHandler>()
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
