using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;

namespace Edo.InformalOrderDocuments.Handlers
{
	/// <summary>
	/// Обработчик неформальных документов заказа
	/// </summary>
	public interface IInformalOrderDocumentHandler
	{
		/// <summary>
		/// Тип документа заказа
		/// </summary>
		OrderDocumentType DocumentType { get; }

		/// <summary>
		/// Обработать документ неформального заказа
		/// </summary>
		/// <param name="order"></param>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<OrderDocumentFileData> ProcessDocumentAsync(OrderEntity order, int documentId, CancellationToken cancellationToken);
	}
}
