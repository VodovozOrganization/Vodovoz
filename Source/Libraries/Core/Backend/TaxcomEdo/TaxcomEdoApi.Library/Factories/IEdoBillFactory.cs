using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoBillFactory
	{
		NonformalizedDocument CreateBillDocument(InfoForCreatingEdoBill data);
		NonformalizedDocument CreateBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
	}
}
