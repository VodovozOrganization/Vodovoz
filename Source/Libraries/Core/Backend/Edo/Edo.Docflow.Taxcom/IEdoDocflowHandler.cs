using System.Threading;
using System.Threading.Tasks;
using Edo.Docflow.Dto;
using Edo.Transport2;
using QS.DomainModel.UoW;
using TaxcomEdo.Contracts.Documents;

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
