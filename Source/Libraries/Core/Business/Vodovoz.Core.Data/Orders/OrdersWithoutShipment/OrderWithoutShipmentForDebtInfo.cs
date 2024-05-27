using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtInfo : OrderWithoutShipmentInfo
	{
		public override OrderDocumentType Type => OrderDocumentType.BillWSForDebt;
	}
}
