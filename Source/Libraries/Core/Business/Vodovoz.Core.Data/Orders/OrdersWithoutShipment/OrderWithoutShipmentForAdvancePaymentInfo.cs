using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForAdvancePaymentInfo : OrderWithoutShipmentInfo
	{
		public override OrderDocumentType Type => OrderDocumentType.BillWSForAdvancePayment;
	}
}
