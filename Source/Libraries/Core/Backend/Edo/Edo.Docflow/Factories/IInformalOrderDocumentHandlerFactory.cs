using Edo.Docflow.Handlers;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Docflow.Factories
{
	public interface IInformalOrderDocumentHandlerFactory
	{
		/// <summary>
		/// Получить обработчик неформального документа по его типу
		/// </summary>
		/// <param name="documentType"></param>
		/// <returns></returns>
		IInformalOrderDocumentHandler GetHandler(OrderDocumentType documentType);
	}
}
