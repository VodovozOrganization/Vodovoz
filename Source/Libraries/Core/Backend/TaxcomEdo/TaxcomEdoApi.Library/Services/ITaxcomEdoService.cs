using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Documents.Events;

namespace TaxcomEdoApi.Library.Services
{
	public interface ITaxcomEdoService
	{
		TaxcomContainer CreateContainerWithUpd(InfoForCreatingEdoUpd infoForCreatingEdoUpd);
		TaxcomContainer CreateContainerWithUpd(UniversalTransferDocumentInfo updInfo);
		TaxcomContainer CreateContainerWithBill(InfoForCreatingEdoBill data);
		TaxcomContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
		SendCustomerInformationEvent GetSendCustomerInformationEvent(string docflowId, string organization);
	}
}
