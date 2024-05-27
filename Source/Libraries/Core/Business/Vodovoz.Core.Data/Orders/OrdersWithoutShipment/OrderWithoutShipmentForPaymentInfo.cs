using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentInfo : OrderWithoutShipmentInfo
	{
		public override OrderDocumentType Type => OrderDocumentType.BillWSForPayment;
	}
}
