using System;
using System.Net.Security;
using System.Security.Authentication;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.Default.Factories;
using CustomerOrdersApi.Library.Default.Repositories;
using CustomerOrdersApi.Library.Default.Services;
using CustomerOrdersApi.Library.V4.Factories;
using CustomerOrdersApi.Library.V4.Repositories;
using CustomerOrdersApi.Library.V4.Services;
using CustomerOrdersApi.Library.V5.Factories;
using CustomerOrdersApi.Library.V5.Factories.DeliveryConditions;
using CustomerOrdersApi.Library.V5.Repositories;
using CustomerOrdersApi.Library.V5.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using RabbitMQ.Client;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position));
			services.Configure<SignatureOptions>(config.GetSection(SignatureOptions.Path));
			services.Configure<CacheOptions>(config.GetSection(CacheOptions.Path));
			
			return services;
		}
		
		public static IServiceCollection AddVersion3(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ICustomerOrderFactory, CustomerOrderFactory>()
				.AddScoped<ICustomerOrdersDiscountService, CustomerOrdersDiscountService>()
				.AddScoped<ICustomerOrderFixedPriceService, CustomerOrderFixedPriceService>()
				.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>()
				.AddScoped<ICustomerOnlineOrderRepository, CustomerOnlineOrderRepository>()
				.AddDefault();
			
			return services;
		}
		
		public static IServiceCollection AddVersion4(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV4, CustomerOrdersServiceV4>()
				.AddScoped<ICustomerOrderFactoryV4, CustomerOrderFactoryV4>()
				.AddScoped<ICustomerOrdersDiscountServiceV4, CustomerOrdersDiscountServiceV4>()
				.AddScoped<ICustomerOrderFixedPriceServiceV4, CustomerOrderFixedPriceServiceV4>()
				.AddScoped<IInfoMessageFactoryV4, InfoMessageFactoryV4>()
				.AddScoped<ICustomerOrderRepositoryV4, CustomerOrderRepositoryV4>()
				.AddScoped<ICustomerOnlineOrderRepositoryV4, CustomerOnlineOrderRepositoryV4>()
				.AddDefault();
			
			return services;
		}
		
		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV5, CustomerOrdersServiceV5>()
				.AddScoped<ICustomerCartService, CustomerCartService>()
				.AddScoped<ICheckUserBasketCacheService, CheckUserBasketCacheService>()
				.AddScoped<ICustomerOrderFactoryV5, CustomerOrderFactoryV5>()
				.AddScoped<IPaymentMethodsCreator, PaymentMethodsCreator>()
				.AddScoped<IDeliveryRulesConditionsCreator, DeliveryRulesConditionsCreator>()
				.AddScoped<IOnlineOrderTemplateConditionsCreator, OnlineOrderTemplateConditionsCreator>()
				.AddScoped<ICustomerOrdersDiscountServiceV5, CustomerOrdersDiscountServiceV5>()
				.AddScoped<ICustomerOrderFixedPriceServiceV5, CustomerOrderFixedPriceServiceV5>()
				.AddScoped<IInfoMessageFactoryV5, InfoMessageFactoryV5>()
				.AddScoped<IWarningMessageFactoryV5, WarningMessageFactoryV5>()
				.AddScoped<VodovozWebSitePaymentMethodFactory>()
				.AddScoped<MobileAppPaymentMethodFactory>()
				.AddScoped<IAdditionalConditionsFactory, AdditionalConditionsFactory>()
				.AddScoped<ICustomerOrderRepositoryV5, CustomerOrderRepositoryV5>()
				.AddScoped<ICustomerOnlineOrderRepositoryV5, CustomerOnlineOrderRepositoryV5>()
				.AddScoped(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddDefault();
			
			return services;
		}
		
		public static IServiceCollection AddDefault(this IServiceCollection services)
		{
			services
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>();

			return services;
		}

		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();

				configurator.Host(
					messageSettings.Host,
					(ushort)messageSettings.Port,
					messageSettings.VirtualHost, hostConfigurator =>
					{
						hostConfigurator.Username(messageSettings.Username);
						hostConfigurator.Password(messageSettings.Password);

						if(messageSettings.UseSSL)
						{
							hostConfigurator.UseSsl(ssl =>
							{
								if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
								{
									ssl.AllowPolicyErrors(allowedPolicyErrors);
								}

								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					});
								
				AddTopologyV3(configurator);
				AddTopologyV4(configurator);
				AddTopologyV5(configurator);

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static void AddTopologyV3(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Send<CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto>(
				x => x.UseRoutingKeyFormatter(y => y.Message.FaultedMessage.ToString()));
			configurator.Message<CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto>(
				x => x.SetEntityName(CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto.ExchangeName));
			configurator.Publish<CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto.ExchangeName,
					"online-orders",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = "False";
					});
				x.BindQueue(
					CustomerOrders.Contracts.Default.Orders.OnlineOrderInfoDto.ExchangeName,
					"online-orders-fault",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = "True";
					});
			});
		}
		
		private static void AddTopologyV4(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<CustomerOrders.Contracts.V4.Orders.CreatingOnlineOrder>(
				x => x.SetEntityName(CustomerOrders.Contracts.V4.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<CustomerOrders.Contracts.V4.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
		
		private static void AddTopologyV5(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<CustomerOrders.Contracts.V5.Orders.CreatingOnlineOrder>(
				x => x.SetEntityName(CustomerOrders.Contracts.V5.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<CustomerOrders.Contracts.V5.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
