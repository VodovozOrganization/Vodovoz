using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml;
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
		string GetSendCustomerInformationEvent(string docflowId, string organization, string updFormat);
		SendOfferCancellationEvent CreateOfferCancellation(string docflowId, string comment);
		SendAcceptCancellationOfferEvent AcceptOfferCancellation(string docflowId);
		SendRejectCancellationOfferEvent RejectOfferCancellation(string docflowId, string comment);
	}
}
