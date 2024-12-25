using Edo.Transport.Messages.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Docflow.Taxcom
{
	public interface IEdoDocflowHandler
	{
		Task CreateTaxcomDocFlowAndSendDocument(TaxcomDocflowSendEvent @event);
		Task<EdoDocflowUpdatedEvent> UpdateOutgoingTaxcomDocFlow(
			OutgoingTaxcomDocflowUpdatedEvent @event, CancellationToken cancellationToken = default);
		Task AcceptIngoingTaxcomEdoDocFlowWaitingForSignature(
			AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent @event, CancellationToken cancellationToken = default);
	}
}
