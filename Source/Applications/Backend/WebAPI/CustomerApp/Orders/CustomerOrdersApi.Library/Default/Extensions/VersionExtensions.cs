using CustomerOrdersApi.Library.Default.Factories;
using CustomerOrdersApi.Library.Default.Repositories;
using CustomerOrdersApi.Library.Default.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace CustomerOrdersApi.Library.Default.Extensions
{
	public static class VersionExtensions
	{
		public static IServiceCollection AddVersion3(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ICustomerOrderFactory, CustomerOrderFactory>()
				.AddScoped<ICustomerOrdersDiscountService, CustomerOrdersDiscountService>()
				.AddScoped<ICustomerOrderFixedPriceService, CustomerOrderFixedPriceService>()
				.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>()
				.AddCommonDependencies();
			
			return services;
		}

		public static void AddTopologyV3(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Send<Dto.Orders.OnlineOrderInfoDto>(
				x => x.UseRoutingKeyFormatter(y => y.Message.FaultedMessage.ToString()));
			configurator.Message<Dto.Orders.OnlineOrderInfoDto>(
				x => x.SetEntityName(Dto.Orders.OnlineOrderInfoDto.ExchangeName));
			configurator.Publish<Dto.Orders.OnlineOrderInfoDto>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					Dto.Orders.OnlineOrderInfoDto.ExchangeName,
					"online-orders",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = "False";
					});
				x.BindQueue(
					Dto.Orders.OnlineOrderInfoDto.ExchangeName,
					"online-orders-fault",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = "True";
					});
			});
		}
	}
}
