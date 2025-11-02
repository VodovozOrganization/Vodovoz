using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Tools.Orders
{
	public interface IEdoContainerMainDocumentIdParser
	{
		/// <summary>
		/// Получение клиента, который отправил документы по ЭДО
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="mainDocumentId">Id главного документа в контейнере(наименование)</param>
		/// <param name="isIncoming">входящий или исходящий контейнер</param>
		/// <returns>Клиент, кто отправил документы</returns>
		Counterparty GetCounterpartyFromMainDocumentId(IUnitOfWork uow, string mainDocumentId, bool isIncoming = true);
	}
}
