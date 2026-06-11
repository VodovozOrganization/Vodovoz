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
using ChangingOrderDtoV5 = CustomerOrdersApi.Library.V5.Dto.Orders.ChangingOrderDto;
using ChangingOrderDtoV6 = CustomerOrdersApi.Library.V6.Dto.Orders.ChangingOrderDto;
using IPaymentRefundServiceFactory = CustomerOrdersApi.Library.Default.Factories.IPaymentRefundServiceFactory;
using PaymentRefundServiceFactory = CustomerOrdersApi.Library.Default.Factories.PaymentRefundServiceFactory;
using IRefundRequestValidatorV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.IRefundRequestValidator;
using RefundRequestValidatorV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.RefundRequestValidator;
using ICloudPaymentsMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.ICloudPaymentsMapper;
using CloudPaymentsMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.CloudPaymentsMapper;
using IPaymentRefundServiceV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.IPaymentRefundService;
using CloudPaymentsRefundServiceV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.CloudPaymentsRefundService;
using YandexPayRefundServiceV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.YandexPayRefundService;
using YooKassaRefundServiceV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.YooKassaRefundService;
using FastPaymentsRefundServiceV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.FastPaymentsRefundService;
using IYandexPayMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.IYandexPayMapper;
using YandexPayMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.YandexPayMapper;
using IYooKassaMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.IYooKassaMapper;
using YooKassaMapperV5 = CustomerOrdersApi.Library.V5.Services.PaymentRefund.Mappers.YooKassaMapper;
using IPaymentRefundServiceFactoryV6 = CustomerOrdersApi.Library.V6.Factories.IPaymentRefundServiceFactory;
using PaymentRefundServiceFactoryV6 = CustomerOrdersApi.Library.V6.Factories.PaymentRefundServiceFactory;
using IRefundRequestValidatorV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.IRefundRequestValidator;
using RefundRequestValidatorV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.RefundRequestValidator;
using ICloudPaymentsMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.ICloudPaymentsMapper;
using CloudPaymentsMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.CloudPaymentsMapper;
using IPaymentRefundServiceV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.IPaymentRefundService;
using CloudPaymentsRefundServiceV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.CloudPaymentsRefundService;
using YandexPayRefundServiceV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.YandexPayRefundService;
using YooKassaRefundServiceV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.YooKassaRefundService;
using FastPaymentsRefundServiceV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.FastPaymentsRefundService;
using IYandexPayMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.IYandexPayMapper;
using YandexPayMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.YandexPayMapper;
using IYooKassaMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.IYooKassaMapper;
using YooKassaMapperV6 = CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers.YooKassaMapper;
using ICustomerOrderCancellationServiceV6 = CustomerOrdersApi.Library.V6.Services.ICustomerOrderCancellationService;
using CustomerOrderCancellationServiceV6 = CustomerOrdersApi.Library.V6.Services.CustomerOrderCancellationService;

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
				.AddScoped<ICustomerOrderCancellationServiceV6, CustomerOrderCancellationServiceV6>()
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

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this ChangingOrderDtoV5 source)
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

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this ChangingOrderDtoV6 source)
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

			return services;
		}

		public static IServiceCollection AddPaymentRefundServicesV5(
			this IServiceCollection services)
		{
			services.AddScoped<IRefundRequestValidatorV5, RefundRequestValidatorV5>();
			services.AddScoped<IPaymentRefundServiceFactory, PaymentRefundServiceFactory>();

			services.AddScoped<ICloudPaymentsMapperV5, CloudPaymentsMapperV5>();
			services.AddScoped<IPaymentRefundServiceV5, CloudPaymentsRefundServiceV5>();

			services.AddScoped<IYandexPayMapperV5, YandexPayMapperV5>();
			services.AddScoped<IPaymentRefundServiceV5, YandexPayRefundServiceV5>();

			services.AddScoped<IYooKassaMapperV5, YooKassaMapperV5>();
			services.AddScoped<IPaymentRefundServiceV5, YooKassaRefundServiceV5>();

			services.AddScoped<IPaymentRefundServiceV5, FastPaymentsRefundServiceV5>();

			return services;
		}

		public static IServiceCollection AddPaymentRefundServicesV6(
			this IServiceCollection services)
		{
			services.AddScoped<IRefundRequestValidatorV6, RefundRequestValidatorV6>();
			services.AddScoped<IPaymentRefundServiceFactoryV6, PaymentRefundServiceFactoryV6>();

			services.AddScoped<ICloudPaymentsMapperV6, CloudPaymentsMapperV6>();
			services.AddScoped<IPaymentRefundServiceV6, CloudPaymentsRefundServiceV6>();

			services.AddScoped<IYandexPayMapperV6, YandexPayMapperV6>();
			services.AddScoped<IPaymentRefundServiceV6, YandexPayRefundServiceV6>();

			services.AddScoped<IYooKassaMapperV6, YooKassaMapperV6>();
			services.AddScoped<IPaymentRefundServiceV6, YooKassaRefundServiceV6>();

			services.AddScoped<IPaymentRefundServiceV6, FastPaymentsRefundServiceV6>();

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
