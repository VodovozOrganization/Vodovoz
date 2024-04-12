using Taxcom.Client.Api.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoBillFactory
	{
		NonformalizedDocument CreateBillDocument(Order order, byte[] attachmentFile, string attachmentName, Organization organization);
		NonformalizedDocument CreateBillWithoutShipmentForAdvancePaymentDocument(OrderWithoutShipmentForAdvancePayment orderWithoutShipmentForAdvancePayment, byte[] attachmentFile, string attachmentName, Organization organization);
		NonformalizedDocument CreateBillWithoutShipmentForDebtDocument(OrderWithoutShipmentForDebt orderWithoutShipmentForDebt, byte[] attachmentFile, string attachmentName, Organization organization);
		NonformalizedDocument CreateBillWithoutShipmentForPaymentDocument(OrderWithoutShipmentForPayment orderWithoutShipmentForPayment, byte[] attachmentFile, string attachmentName, Organization organization);
	}
}
