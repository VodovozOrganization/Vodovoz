using System;
using System.Net.Security;
using System.Security.Authentication;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position));
			
			return services;
		}
		
		public static IServiceCollection AddVersion4(this IServiceCollection services)
		{
			services.AddScoped<V4.Services.ICustomerOrdersServiceV4, V4.Services.CustomerOrdersServiceV4>()
				.AddScoped<V4.Factories.ICustomerOrderFactoryV4, V4.Factories.CustomerOrderFactoryV4>()
				.AddScoped<V4.Factories.IInfoMessageFactoryV4, V4.Factories.InfoMessageFactoryV4>()
				.AddDefaultServices();
			
			return services;
		}
		
		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			services.AddScoped<V5.Services.ICustomerOrdersServiceV5, V5.Services.CustomerOrdersServiceV5>()
				.AddScoped<V5.Factories.ICustomerOrderFactoryV5, V5.Factories.CustomerOrderFactoryV5>()
				.AddScoped<V5.Factories.IInfoMessageFactoryV5, V5.Factories.InfoMessageFactoryV5>()
				.AddDefaultServices();
			
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
				configurator.Message<OnlineOrderInfoDto>(x => x.SetEntityName(OnlineOrderInfoDto.ExchangeName));
				configurator.Publish<OnlineOrderInfoDto>(x =>
				{
					x.ExchangeType = ExchangeType.Direct;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						OnlineOrderInfoDto.ExchangeName,
						"online-orders",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "False";
						});
					x.BindQueue(
						OnlineOrderInfoDto.ExchangeName,
						"online-orders-fault",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "True";
						});
				});
				
				configurator.Message<CreatingOnlineOrder>(x => x.SetEntityName(CreatingOnlineOrder.ExchangeAndQueueName));
				configurator.Publish<CreatingOnlineOrder>(x =>
				{
					x.ExchangeType = ExchangeType.Fanout;
					x.Durable = true;
					x.AutoDelete = false;
				});
								
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this ChangingOrderDto source)
		{
			return new UpdateOnlineOrderFromChangeRequest
			{
				OnlineOrderId = source.OnlineOrderId,
				OnlinePayment = source.OnlinePayment,
				IsFastDelivery = source.IsFastDelivery,
				Source = source.Source,
				PaymentStatus = source.PaymentStatus,
				OnlinePaymentSource = source.OnlinePaymentSource,
				CounterpartyErpId = source.CounterpartyErpId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType,
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId
			};
		}
		
		private static IServiceCollection AddDefaultServices(this IServiceCollection services)
		{
			services
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>();
			
			return services;
		}
	}
}
