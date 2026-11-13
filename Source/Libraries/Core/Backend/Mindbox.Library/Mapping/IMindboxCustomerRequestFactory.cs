using Mindbox.Library.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Формирует тело запроса на создание/обновление клиента в Mindbox
	/// </summary>
	public interface IMindboxCustomerRequestFactory
	{
		/// <summary>
		/// Создаёт тело запроса с данными контрагента
		/// </summary>
		/// <param name="unitOfWork">Unit of work для доступа к связанным сущностям контрагента</param>
		/// <param name="counterparty">Контрагент, данные которого отправляются в Mindbox</param>
		/// <returns>Тело запроса на создание/обновление клиента</returns>
		MindboxCustomerRequest Create(IUnitOfWork unitOfWork, CounterpartyEntity counterparty);
	}
}
