using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Models.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoBillFactory
	{
		NonformalizedDocument CreateBillDocument(InfoForCreatingEdoBill data);
		NonformalizedDocument CreateBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
	}
}
