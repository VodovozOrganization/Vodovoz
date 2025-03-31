using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Services
{
	public interface ITaxcomEdoService
	{
		TaxcomContainer CreateContainerWithUpd(InfoForCreatingEdoUpd infoForCreatingEdoUpd);
		TaxcomContainer CreateContainerWithUpd(UniversalTransferDocumentInfo updInfo);
		TaxcomContainer CreateContainerWithBill(InfoForCreatingEdoBill data);
		TaxcomContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
		string GetSendCustomerInformationEvent(string docflowId, string organization, string updFormat);
	}
}
