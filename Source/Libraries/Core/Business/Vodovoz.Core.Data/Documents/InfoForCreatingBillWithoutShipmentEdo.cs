using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Data.Documents
{
	public class InfoForCreatingBillWithoutShipmentEdo : InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingBillWithoutShipmentEdo(OrderWithoutShipmentInfo orderWithoutShipmentInfo)
		{
			OrderWithoutShipmentInfo = orderWithoutShipmentInfo;
		}
		
		public OrderWithoutShipmentInfo OrderWithoutShipmentInfo { get; }

		public static InfoForCreatingBillWithoutShipmentEdo Create(OrderWithoutShipmentInfo orderWithoutShipmentInfo) =>
			new InfoForCreatingBillWithoutShipmentEdo(orderWithoutShipmentInfo);
	}
}
