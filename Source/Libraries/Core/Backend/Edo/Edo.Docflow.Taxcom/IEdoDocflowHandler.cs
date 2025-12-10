using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Docflow.Taxcom
{
	public interface IEdoDocflowHandler
	{
		Task CreateTaxcomDocFlowAndSendDocument(TaxcomDocflowSendEvent @event);

		/// <summary>
		/// Создание и отправка неформализованного документа в документооборот Такском
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task CreateTaxcomDocflowInformalDocument(TaxcomDocflowInformalDocumentSendEvent @event);

		Task<EdoDocflowUpdatedEvent> UpdateOutgoingTaxcomDocFlow(
			OutgoingTaxcomDocflowUpdatedEvent @event, 
			CancellationToken cancellationToken = default
		);

		Task AcceptIngoingTaxcomEdoDocFlowWaitingForSignature(
			AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent @event, 
			CancellationToken cancellationToken = default
		);

		Task SendOfferCancellation(
			TaxcomDocflowRequestCancellationEvent @event,
			CancellationToken cancellationToken
		);

		Task AcceptOfferCancellation(
			AcceptingWaitingForCancellationDocflowEvent @event, 
			CancellationToken cancellationToken
		);
	}
}
