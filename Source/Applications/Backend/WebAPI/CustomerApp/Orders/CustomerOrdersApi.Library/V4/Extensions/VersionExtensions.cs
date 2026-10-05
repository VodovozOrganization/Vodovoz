using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Services.Import;
using CustomerOrdersApi.Library.V4.Factories;
using CustomerOrdersApi.Library.V4.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Extensions
{
	public static class VersionExtensions
	{
		public static IServiceCollection AddVersion4(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersServiceV4, CustomerOrdersServiceV4>()
				.AddScoped<ICustomerOrderFactoryV4, CustomerOrderFactoryV4>()
				.AddScoped<ICustomerOrdersDiscountServiceV4, CustomerOrdersDiscountServiceV4>()
				.AddScoped<ICustomerOrderFixedPriceServiceV4, CustomerOrderFixedPriceServiceV4>()
				.AddScoped<IInfoMessageFactory, InfoMessageFactory>()
				.AddScoped<Repositories.ICustomerOrderRepository, Repositories.CustomerOrderRepository>()
				.AddScoped<ISiteOrdersImportRequestValidator, SiteOrdersImportRequestValidator>()
				.AddScoped<ISiteOrdersImportService, SiteOrdersImportService>()
				.AddScoped<IOnlineOrderFixedPriceHandler, OnlineOrderFixedPriceHandler>()
				.AddScoped<IOnlineOrderDiscountHandler, OnlineOrderDiscountHandler>()
				.AddScoped<IApplicablePromotionFactory, ApplicablePromotionFactory>()
				.AddCommonDependencies();
			
			return services;
		}

		public static void AddTopologyV4(IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<CreatingOnlineOrder>(x => x.SetEntityName(CreatingOnlineOrder.ExchangeAndQueueName));
			configurator.Publish<CreatingOnlineOrder>(x =>
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
	}
}
