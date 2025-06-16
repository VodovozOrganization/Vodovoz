using DriverApi.Notifications.Client;
using Microsoft.Extensions.DependencyInjection;
using RevenueService.Client;
using TrueMarkApi.Client;
using Vodovoz.Application.Complaints;
using Vodovoz.Application.Contacts;
using Vodovoz.Application.FileStorage;
using Vodovoz.Application.Goods;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Pacs;
using Vodovoz.Application.Payments;
using Vodovoz.Application.Services;
using Vodovoz.Application.Services.Subdivisions;
using Vodovoz.Application.TrueMark;
using Vodovoz.Application.Users;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Service;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Subdivisions;
using VodovozBusiness.Services.TrueMark;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddScoped<IRouteOptimizer, RouteOptimizer>()
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
			.AddScoped<IPaymentService, PaymentService>()
			.AddScoped<IOrderService, OrderService>()
			.AddScoped<IPhoneService, PhoneService>()
			.AddScoped<INomenclatureService, NomenclatureService>()
			.AddScoped<IComplaintService, ComplaintService>()
			.AddScoped<ISubdivisionPermissionsService, SubdivisionPermissionsService>()
			.AddScoped<ITrueMarkWaterCodeService, TrueMarkWaterCodeService>()
			.AddScoped<ITrueMarkTransportCodeFactory, TrueMarkTransportCodeFactory>()
			.AddScoped<ITrueMarkWaterGroupCodeFactory, TrueMarkWaterGroupCodeFactory>()
			.AddScoped<ITrueMarkWaterIdentificationCodeFactory, TrueMarkWaterIdentificationCodeFactory>()
			.AddScoped<IRecomendationService, RecomendationService>()
			.AddTrueMarkApiClient()
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
