using System.ComponentModel;
using CustomerOrdersApi.Library.V8.Dto.Orders.RequestsForCall;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale.RequestsForCall;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class VersionExtensions
	{
		public static IServiceCollection AddVersion8(this IServiceCollection services)
		{
			services
				.AddScoped<Services.ICustomerOrdersService, Services.CustomerOrdersService>()
				.AddScoped<Services.ICustomerOrdersDiscountService, Services.CustomerOrdersDiscountService>()
				.AddScoped<Services.ICustomerOrderFixedPriceService, Services.CustomerOrderFixedPriceService>()
				.AddScoped<Services.ICustomerOrderCancellationService, Services.CustomerOrderCancellationService>()
				.AddScoped<Services.ICourierTrackingService, Services.CourierTrackingService>()
				.AddScoped<Factories.ICustomerOrderFactory, Factories.CustomerOrderFactory>()
				.AddScoped<Factories.IInfoMessageFactory, Factories.InfoMessageFactory>()
				.AddScoped<Factories.IOnlineOrderItemDtoFactory, Factories.OnlineOrderItemDtoFactory>()
				.AddScoped<Repositories.ICustomerOrderRepository, Repositories.CustomerOrderRepository>()
				.AddCommonDependencies()
				.AddPaymentRefundServices()
				;

			return services;
		}
		
		private static void AddTopologyV8(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<Dto.Orders.CreatingOnlineOrder>(x => x.SetEntityName(Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<Dto.Orders.CreatingOnlineOrder>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		public static RequestForCallBase ToRequestForCall(
			this CreatingRequestForCallDto source,
			Nomenclature nomenclature,
			Counterparty counterparty
			)
		{
			switch(source.Type)
			{
				case Dto.Orders.RequestsForCall.RequestForCallType.General:
					return RequestForCall.Create(
						source.Source,
						source.ContactName,
						source.PhoneNumber,
						nomenclature,
						counterparty
					);
				case Dto.Orders.RequestsForCall.RequestForCallType.Service:
					return ServiceRequestForCall.Create(
						source.Source,
						source.ContactName,
						source.PhoneNumber,
						nomenclature,
						counterparty
					);
				default:
					throw new InvalidEnumArgumentException(
						$"Неизвестный тип заявки на звонок {source.Type}. Нужно добавить это значение в {nameof(Dto.Orders.RequestsForCall.RequestForCallType)} апи заказов");
			}
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
			services.AddScoped<Factories.IPaymentRefundServiceFactory, Factories.PaymentRefundServiceFactory>();

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
