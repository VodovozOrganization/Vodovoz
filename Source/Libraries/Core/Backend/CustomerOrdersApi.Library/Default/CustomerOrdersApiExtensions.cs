using System;
using System.Net.Security;
using System.Security.Authentication;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using CustomerOrdersApi.Library.Default.Factories;
using CustomerOrdersApi.Library.Default.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Handlers;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.Default
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services
				.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position))
				.Configure<SignatureOptions>(config.GetSection(SignatureOptions.Path));
			
			return services;
		}
		
		public static IServiceCollection AddDependenciesGroup(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ICustomerOrdersDiscountService, CustomerOrdersDiscountService>()
				.AddScoped<ICustomerOrderFixedPriceService, CustomerOrderFixedPriceService>()
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<ICustomerOrderFactory, CustomerOrderFactory>()
				.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>()
				.AddScoped<IOnlineOrderDiscountHandler, OnlineOrderDiscountHandler>()
				.AddScoped<IOnlineOrderFixedPriceHandler, OnlineOrderFixedPriceHandler>();
			
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
								
				configurator.Send<OnlineOrderInfoDto>(x => x.UseRoutingKeyFormatter(y => y.Message.FaultedMessage.ToString()));
				configurator.Message<OnlineOrderInfoDto>(x => x.SetEntityName("online-order-received"));
				configurator.Publish<OnlineOrderInfoDto>(x =>
				{
					x.ExchangeType = ExchangeType.Direct;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						"online-order-received",
						"online-orders",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "False";
						});
					x.BindQueue(
						"online-order-received",
						"online-orders-fault",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "True";
						});
				});
								
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}
	}
}
