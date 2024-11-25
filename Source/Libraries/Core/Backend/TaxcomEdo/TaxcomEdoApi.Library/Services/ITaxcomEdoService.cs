using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Services
{
	public interface ITaxcomEdoService
	{
		TaxcomContainer CreateContainerWithUpd(InfoForCreatingEdoUpd infoForCreatingEdoUpd);
		TaxcomContainer CreateContainerWithBill(InfoForCreatingEdoBill data);
		TaxcomContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
	}
}
