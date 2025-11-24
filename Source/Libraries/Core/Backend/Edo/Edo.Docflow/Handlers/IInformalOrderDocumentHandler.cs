using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Docflow.Handlers
{
	public interface IInformalOrderDocumentHandler
	{
		/// <summary>
		/// Тип документа заказа
		/// </summary>
		OrderDocumentType DocumentType { get; }

		/// <summary>
		/// Обработка документа заказа
		/// </summary>
		/// <param name="order"></param>
		/// <param name="fileData"></param>
		/// <param name="documentId"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		Task<InfoForCreatingEdoEquipmentTransfer> ProcessDocument(
			OrderEntity order,
			OrderDocumentFileData fileData,
			int documentId,
			CancellationToken cancellationToken);
	}
}
