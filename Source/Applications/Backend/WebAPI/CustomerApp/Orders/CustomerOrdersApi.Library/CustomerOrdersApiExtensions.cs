using CloudPaymentsApi.Client;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.Default.Factories;
using CustomerOrdersApi.Library.Default.Services;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Factories;
using CustomerOrdersApi.Library.V4.Services;
using CustomerOrdersApi.Library.V5.Factories;
using CustomerOrdersApi.Library.V5.Services;
using CustomerOrdersApi.Library.V6.Factories;
using CustomerOrdersApi.Library.V6.Services;
using FastPaymentsApi.Client;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;
using YandexPayApi.Client;
using YooKassaApi.Client;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position));
			services.Configure<SignatureOptions>(config.GetSection(SignatureOptions.Path));
			
			return services;
		}
		
		public static IServiceCollection AddVersion3(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ICustomerOrderFactory, CustomerOrderFactory>()
				.AddScoped<ICustomerOrdersDiscountService, CustomerOrdersDiscountService>()
				.AddScoped<ICustomerOrderFixedPriceService, CustomerOrderFixedPriceService>()
				.AddDefault();
			
			return services;
		}
		
		public static IServiceCollection AddVersion4(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV4, CustomerOrdersServiceV4>()
				.AddScoped<ICustomerOrderFactoryV4, CustomerOrderFactoryV4>()
				.AddScoped<ICustomerOrdersDiscountServiceV4, CustomerOrdersDiscountServiceV4>()
				.AddScoped<ICustomerOrderFixedPriceServiceV4, CustomerOrderFixedPriceServiceV4>()
				.AddScoped<IInfoMessageFactory, InfoMessageFactory>()
				.AddDefault();
			
			return services;
		}
		
		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV5, CustomerOrdersServiceV5>()
				.AddScoped<ICustomerOrderFactoryV5, CustomerOrderFactoryV5>()
				.AddScoped<ICustomerOrdersDiscountServiceV5, CustomerOrdersDiscountServiceV5>()
				.AddScoped<ICustomerOrderFixedPriceServiceV5, CustomerOrderFixedPriceServiceV5>()
				.AddScoped<V5.Services.ICustomerOrderCancellationService, V5.Services.CustomerOrderCancellationService>()
				.AddScoped<IInfoMessageFactoryV5, InfoMessageFactoryV5>()
				.AddDefault();
			
			return services;
		}

		public static IServiceCollection AddVersion6(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV6, CustomerOrdersServiceV6>()
				.AddScoped<ICustomerOrderFactoryV6, CustomerOrderFactoryV6>()
				.AddScoped<ICustomerOrdersDiscountServiceV6, CustomerOrdersDiscountServiceV6>()
				.AddScoped<ICustomerOrderFixedPriceServiceV6, CustomerOrderFixedPriceServiceV6>()
				.AddScoped<IInfoMessageFactoryV6, InfoMessageFactoryV6>()
				.AddScoped<V6.Services.ICustomerOrderCancellationService, V6.Services.CustomerOrderCancellationService>()
				.AddScoped<V6.Services.ICourierTrackingService, V6.Services.CourierTrackingService>()
				.AddDefault();

			return services;
		}
		
		public static IServiceCollection AddVersion7(this IServiceCollection services)
		{
			services
				.AddScoped<V7.Services.ICustomerOrdersService, V7.Services.CustomerOrdersService>()
				.AddScoped<V7.Services.ICustomerOrdersDiscountService, V7.Services.CustomerOrdersDiscountService>()
				.AddScoped<V7.Services.ICustomerOrderFixedPriceService, V7.Services.CustomerOrderFixedPriceService>()
				.AddScoped<V7.Services.ICustomerOrderCancellationService, V7.Services.CustomerOrderCancellationService>()
				.AddScoped<V7.Services.ICourierTrackingService, V7.Services.CourierTrackingService>()
				.AddScoped<V7.Factories.ICustomerOrderFactory, V7.Factories.CustomerOrderFactory>()
				.AddScoped<V7.Factories.IInfoMessageFactory, V7.Factories.InfoMessageFactory>()
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
				AddTopologyV6(configurator);
				AddTopologyV7(configurator);

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static void AddTopologyV3(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Send<Default.Dto.Orders.OnlineOrderInfoDto>(
				x => x.UseRoutingKeyFormatter(y => y.Message.FaultedMessage.ToString()));
			configurator.Message<Default.Dto.Orders.OnlineOrderInfoDto>(
				x => x.SetEntityName(Default.Dto.Orders.OnlineOrderInfoDto.ExchangeName));
			configurator.Publish<Default.Dto.Orders.OnlineOrderInfoDto>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					Default.Dto.Orders.OnlineOrderInfoDto.ExchangeName,
					"online-orders",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = "False";
					});
				x.BindQueue(
					Default.Dto.Orders.OnlineOrderInfoDto.ExchangeName,
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
			configurator.Message<CreatingOnlineOrder>(x => x.SetEntityName(CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		private static void AddTopologyV5(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<V5.Dto.Orders.CreatingOnlineOrder>(x => x.SetEntityName(V5.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<V5.Dto.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		private static void AddTopologyV6(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<V6.Dto.Orders.CreatingOnlineOrder>(x => x.SetEntityName(V6.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<V6.Dto.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
		
		private static void AddTopologyV7(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<V7.Dto.Orders.CreatingOnlineOrder>(x => x.SetEntityName(V7.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<V7.Dto.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
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
				ErpCounterpartyId = source.ErpCounterpartyId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType,
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId
			};
		}

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this V5.Dto.Orders.ChangingOrderDto source)
		{
			return new UpdateOnlineOrderFromChangeRequest
			{
				OnlineOrderId = source.OnlineOrderId,
				OnlinePayment = source.OnlinePayment,
				IsFastDelivery = source.IsFastDelivery,
				Source = source.Source,
				PaymentStatus = source.PaymentStatus,
				OnlinePaymentSource = source.OnlinePaymentSource,
				ErpCounterpartyId = source.ErpCounterpartyId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType,
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId,
				TransactionId = source.TransactionId
			};
		}

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this V6.Dto.Orders.ChangingOrderDto source)
		{
			return new UpdateOnlineOrderFromChangeRequest
			{
				OnlineOrderId = source.OnlineOrderId,
				OnlinePayment = source.OnlinePayment,
				IsFastDelivery = source.IsFastDelivery,
				Source = source.Source,
				PaymentStatus = source.PaymentStatus,
				OnlinePaymentSource = source.OnlinePaymentSource,
				ErpCounterpartyId = source.ErpCounterpartyId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType,
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId,
				TransactionId = source.TransactionId
			};
		}
		
		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this V7.Dto.Orders.ChangingOrderDto source)
		{
			return new UpdateOnlineOrderFromChangeRequest
			{
				OnlineOrderId = source.OnlineOrderId,
				OnlinePayment = source.OnlinePayment,
				IsFastDelivery = source.IsFastDelivery,
				Source = source.Source,
				PaymentStatus = source.PaymentStatus,
				OnlinePaymentSource = source.OnlinePaymentSource,
				ErpCounterpartyId = source.ErpCounterpartyId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType,
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId,
				TransactionId = source.TransactionId
			};
		}

		public static IServiceCollection AddPaymentRefundServices(
			this IServiceCollection services)
		{
			services.AddPaymentRefundServicesV5();
			services.AddPaymentRefundServicesV6();
			services.AddPaymentRefundServicesV7();

			return services;
		}

		public static IServiceCollection AddPaymentRefundServicesV5(
			this IServiceCollection services)
		{
			services.AddScoped<V5.Services.PaymentRefund.IRefundRequestValidator, V5.Services.PaymentRefund.RefundRequestValidator>();
			services.AddScoped<V5.Factories.IPaymentRefundServiceFactory, V5.Factories.PaymentRefundServiceFactory>();

			services.AddScoped<V5.Services.PaymentRefund.Mappers.ICloudPaymentsMapper, V5.Services.PaymentRefund.Mappers.CloudPaymentsMapper>();
			services.AddScoped<V5.Services.PaymentRefund.IPaymentRefundService, V5.Services.PaymentRefund.CloudPaymentsRefundService>();

			services.AddScoped<V5.Services.PaymentRefund.Mappers.IYandexPayMapper, V5.Services.PaymentRefund.Mappers.YandexPayMapper>();
			services.AddScoped<V5.Services.PaymentRefund.IPaymentRefundService, V5.Services.PaymentRefund.YandexPayRefundService>();

			services.AddScoped<V5.Services.PaymentRefund.Mappers.IYooKassaMapper, V5.Services.PaymentRefund.Mappers.YooKassaMapper>();
			services.AddScoped<V5.Services.PaymentRefund.IPaymentRefundService, V5.Services.PaymentRefund.YooKassaRefundService>();

			services.AddScoped<V5.Services.PaymentRefund.IPaymentRefundService, V5.Services.PaymentRefund.FastPaymentsRefundService>();

			return services;
		}

		public static IServiceCollection AddPaymentRefundServicesV6(
			this IServiceCollection services)
		{
			services.AddScoped<V6.Services.PaymentRefund.IRefundRequestValidator, V6.Services.PaymentRefund.RefundRequestValidator>();
			services.AddScoped<V6.Factories.IPaymentRefundServiceFactory, V6.Factories.PaymentRefundServiceFactory>();

			services.AddScoped<V6.Services.PaymentRefund.Mappers.ICloudPaymentsMapper, V6.Services.PaymentRefund.Mappers.CloudPaymentsMapper>();
			services.AddScoped<V6.Services.PaymentRefund.IPaymentRefundService, V6.Services.PaymentRefund.CloudPaymentsRefundService>();

			services.AddScoped<V6.Services.PaymentRefund.Mappers.IYandexPayMapper, V6.Services.PaymentRefund.Mappers.YandexPayMapper>();
			services.AddScoped<V6.Services.PaymentRefund.IPaymentRefundService, V6.Services.PaymentRefund.YandexPayRefundService>();

			services.AddScoped<V6.Services.PaymentRefund.Mappers.IYooKassaMapper, V6.Services.PaymentRefund.Mappers.YooKassaMapper>();
			services.AddScoped<V6.Services.PaymentRefund.IPaymentRefundService, V6.Services.PaymentRefund.YooKassaRefundService>();

			services.AddScoped<V6.Services.PaymentRefund.IPaymentRefundService, V6.Services.PaymentRefund.FastPaymentsRefundService>();

			return services;
		}
		
		public static IServiceCollection AddPaymentRefundServicesV7(this IServiceCollection services)
		{
			services.AddScoped<V7.Services.PaymentRefund.IRefundRequestValidator, V7.Services.PaymentRefund.RefundRequestValidator>();
			services.AddScoped<V7.Factories.IPaymentRefundServiceFactory, V7.Factories.PaymentRefundServiceFactory>();

			services.AddScoped<V7.Services.PaymentRefund.Mappers.ICloudPaymentsMapper, V7.Services.PaymentRefund.Mappers.CloudPaymentsMapper>();
			services.AddScoped<V7.Services.PaymentRefund.IPaymentRefundService, V7.Services.PaymentRefund.CloudPaymentsRefundService>();

			services.AddScoped<V7.Services.PaymentRefund.Mappers.IYandexPayMapper, V7.Services.PaymentRefund.Mappers.YandexPayMapper>();
			services.AddScoped<V7.Services.PaymentRefund.IPaymentRefundService, V7.Services.PaymentRefund.YandexPayRefundService>();

			services.AddScoped<V7.Services.PaymentRefund.Mappers.IYooKassaMapper, V7.Services.PaymentRefund.Mappers.YooKassaMapper>();
			services.AddScoped<V7.Services.PaymentRefund.IPaymentRefundService, V7.Services.PaymentRefund.YooKassaRefundService>();

			services.AddScoped<V7.Services.PaymentRefund.IPaymentRefundService, V7.Services.PaymentRefund.FastPaymentsRefundService>();

			return services;
		}

		public static IServiceCollection AddPaymentApiClients(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddSingleton<JsonSerializerOptions>(sp =>
			{
				return new JsonSerializerOptions
				{
					WriteIndented = false,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				};
			});

			services
				.AddFastPaymentsApiClient(configuration)
				.AddCloudPaymentsApiClient(configuration)
				.AddYandexPayApiClient(configuration)
				.AddYooKassaApiClient(configuration);

			return services;
		}
	}
}
