using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Documents.Events;
using TaxcomEdoApi.Library.Models.Containers;

namespace TaxcomEdoApi.Library.Services.Interfaces
{
	public interface ITaxcomEdoService
	{
		NewContainer CreateContainerWithUpd(UniversalTransferDocumentInfo updInfo);
		NewContainer CreateContainerWithBill(InfoForCreatingEdoBill data);
		NewContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data);
		string GetSendCustomerInformationEvent(string docflowId, string organization, string updFormat);
		SendOfferCancellationEvent CreateOfferCancellation(string docflowId, string comment);
		SendAcceptCancellationOfferEvent AcceptOfferCancellation(string docflowId);
		SendRejectCancellationOfferEvent RejectOfferCancellation(string docflowId, string comment);
	}
}
