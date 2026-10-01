using CustomerOrdersApi.Library.V4.Services.Import;
using CustomerOrdersApi.Library.V5.Factories;
using CustomerOrdersApi.Library.V5.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Extensions
{
	public static class VersionExtensions
	{
		public static IServiceCollection AddVersion5(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV5, CustomerOrdersServiceV5>()
				.AddScoped<ICustomerOrderFactoryV5, CustomerOrderFactoryV5>()
				.AddScoped<ICustomerOrdersDiscountServiceV5, CustomerOrdersDiscountServiceV5>()
				.AddScoped<ICustomerOrderFixedPriceServiceV5, CustomerOrderFixedPriceServiceV5>()
				.AddScoped<ICustomerOrderCancellationService, CustomerOrderCancellationService>()
				.AddScoped<IInfoMessageFactoryV5, InfoMessageFactoryV5>()
				.AddScoped<Repositories.ICustomerOrderRepository, Repositories.CustomerOrderRepository>()
				.AddScoped<ISiteOrdersImportRequestValidator, SiteOrdersImportRequestValidator>()
				.AddScoped<ISiteOrdersImportService, SiteOrdersImportService>()
				.AddCommonDependencies()
				.AddPaymentRefundServices()
				;
			
			return services;
		}

		public static void AddTopologyV5(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<Dto.Orders.CreatingOnlineOrder>(x => x.SetEntityName(Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<Dto.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this Dto.Orders.ChangingOrderDto source)
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

		private static IServiceCollection AddPaymentRefundServices(this IServiceCollection services)
		{
			services.AddScoped<Services.PaymentRefund.IRefundRequestValidator, Services.PaymentRefund.RefundRequestValidator>();
			services.AddScoped<IPaymentRefundServiceFactory, PaymentRefundServiceFactory>();

			services.AddScoped<Services.PaymentRefund.Mappers.ICloudPaymentsMapper, Services.PaymentRefund.Mappers.CloudPaymentsMapper>();
			services.AddScoped<Services.PaymentRefund.IPaymentRefundService, Services.PaymentRefund.CloudPaymentsRefundService>();

			services.AddScoped<Services.PaymentRefund.Mappers.IYandexPayMapper, Services.PaymentRefund.Mappers.YandexPayMapper>();
			services.AddScoped<Services.PaymentRefund.IPaymentRefundService, Services.PaymentRefund.YandexPayRefundService>();

			services.AddScoped<Services.PaymentRefund.Mappers.IYooKassaMapper, Services.PaymentRefund.Mappers.YooKassaMapper>();
			services.AddScoped<Services.PaymentRefund.IPaymentRefundService, Services.PaymentRefund.YooKassaRefundService>();

			services.AddScoped<Services.PaymentRefund.IPaymentRefundService, Services.PaymentRefund.FastPaymentsRefundService>();

			return services;
		}
	}
}
