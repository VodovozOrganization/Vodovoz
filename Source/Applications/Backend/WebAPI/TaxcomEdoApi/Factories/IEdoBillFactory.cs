using Taxcom.Client.Api.Entity;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoBillFactory
	{
		NonformalizedDocument CreateBillDocument(OrderInfoForEdo orderInfoForEdo, byte[] attachmentFile, string attachmentName);
		NonformalizedDocument CreateBillWithoutShipment(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachmentFile, string attachmentName);
	}
}
