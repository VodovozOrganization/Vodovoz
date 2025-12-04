using Edo.InformalOrderDocuments.Handlers;
using Vodovoz.Core.Domain.Orders;

namespace Edo.InformalOrderDocuments.Factories
{
	/// <summary>
	/// Фабрика для создания обработчиков неформализованных документов по типу документа
	/// </summary>
	public interface IInformalOrderDocumentHandlerFactory
	{
		/// <summary>
		/// Получить обработчик неформализованного документа по типу документа
		/// </summary>
		/// <param name="documentType"></param>
		/// <returns></returns>
		IInformalOrderDocumentHandler GetHandler(OrderDocumentType documentType);
	}
}
