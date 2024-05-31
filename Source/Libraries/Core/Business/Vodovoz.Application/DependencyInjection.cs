﻿using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Goods;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Application.Pacs;
using Vodovoz.Application.Payments;
using Vodovoz.Application.Services;
using Vodovoz.Domain.Service;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddScoped<IRouteOptimizer, RouteOptimizer>()
			.AddApplicationServices();

		public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
			.AddSingleton<OperatorService>()
			.AddScoped<ICounterpartyService, CounterpartyService>()
			.AddScoped<IRouteListService, RouteListService>()
			.AddScoped<IPaymentService, PaymentService>()
			.AddScoped<IOrderService, OrderService>()
			.AddScoped<INomenclatureService, NomenclatureService>()
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
		;
	}
}
