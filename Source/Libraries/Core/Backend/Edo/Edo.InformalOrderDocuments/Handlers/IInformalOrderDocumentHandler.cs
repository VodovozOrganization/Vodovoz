using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;

namespace Edo.InformalOrderDocuments.Handlers
{
	public interface IInformalOrderDocumentHandler
	{
		OrderDocumentType DocumentType { get; }

		Task<OrderDocumentFileData> ProcessDocumentAsync(OrderEntity order, int documentId, CancellationToken cancellationToken);
	}
}
