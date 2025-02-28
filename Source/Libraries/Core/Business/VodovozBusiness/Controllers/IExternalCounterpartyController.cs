using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		/// <summary>
		/// Сообщение, транслируемое при привязанных пользователях ИПЗ к номеру телефона
		/// </summary>
		string PhoneAssignedExternalCounterpartyMessage { get; }
		/// <summary>
		/// Удаление пользователя ИПЗ и всей связанной информации
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phoneId">Id телефона</param>
		/// <returns></returns>
		bool DeleteExternalCounterparties(IUnitOfWork uow, int phoneId);
		/// <summary>
		/// Удаление пользователя ИПЗ и всей связанной информации
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="externalCounterparties">Список удаляемых пользователей ИПЗ</param>
		void DeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<ExternalCounterparty> externalCounterparties);
		/// <summary>
		/// Проверка наличия привязанных пользователей к номеру телефона
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phoneId">Id телефона</param>
		/// <param name="externalCounterparties">Возвращаемый список привязанных пользователей ИПЗ</param>
		/// <returns></returns>
		bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IEnumerable<ExternalCounterparty> externalCounterparties);
		/// <summary>
		/// Получение информации о привязанных пользователях ИПЗ по Id клиента
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Id клиента</param>
		/// <returns></returns>
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(IUnitOfWork uow, int counterpartyId);
		/// <summary>
		/// Получение информации о привязанных пользователях ИПЗ по Id телефонов
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phonesIds">Список Id телефонов</param>
		/// <returns></returns>
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(IUnitOfWork uow, IEnumerable<int> phonesIds);
	}
}
