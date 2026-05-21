using CustomerOrders.Contracts.V5.Orders;
using CustomerOrdersApi.Library.Extensions;
using Vodovoz.Core.Domain.Orders;
using VodovozBusiness.Extensions;

namespace CustomerOrdersApi.Library.V6
{
	public static class DtoExtensions
	{
		public static UpdateOnlineOrderFromChangeRequest ToUpdateOnlineOrderFromChangeRequest(this ChangingOrderDto source)
		{
			return new UpdateOnlineOrderFromChangeRequest
			{
				OnlineOrderId = source.OnlineOrderId,
				OnlinePayment = source.OnlinePayment,
				IsFastDelivery = source.IsFastDelivery,
				Source = source.Source.ToSource(),
				PaymentStatus = source.PaymentStatus.ToOnlineOrderPaymentStatus(),
				OnlinePaymentSource = source.OnlinePaymentSource.ToOnlinePaymentSource(),
				ErpCounterpartyId = source.ErpCounterpartyId,
				ExternalCounterpartyId = source.ExternalCounterpartyId,
				OnlineOrderPaymentType = source.OnlineOrderPaymentType.ToOnlineOrderPaymentType(),
				UnPaidReason = source.UnPaidReason,
				DeliveryDate = source.DeliveryDate,
				DeliveryScheduleId = source.DeliveryScheduleId
			};
		}
	}
}
