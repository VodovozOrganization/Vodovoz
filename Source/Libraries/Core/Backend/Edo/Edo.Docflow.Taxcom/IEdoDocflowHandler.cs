using System.Threading;
using System.Threading.Tasks;
using Edo.Docflow.Dto;
using QS.DomainModel.UoW;
using TaxcomEdo.Contracts.Documents;

namespace Edo.Docflow.Taxcom
{
	public interface IEdoDocflowHandler
	{
		Task CreateTaxcomDocFlowAndSendDocument(
			IUnitOfWork uow,
			int edoOutgoingDocumentId,
			UniversalTransferDocumentInfo updInfo);

		Task UpdateOutgoingTaxcomDocFlow(IUnitOfWork uow, EdoDocFlow edoDocFlow, CancellationToken cancellationToken);
		Task ProcessIngoingTaxcomEdoDocFlow(IUnitOfWork uow, EdoDocFlow edoDocFlow, CancellationToken cancellationToken);
	}
}
