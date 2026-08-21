using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using VodovozBusiness.Nodes;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyRepository
	{
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, Guid externalCounterpartyId, CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber, CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom);
		IList<ExternalCounterparty> GetExternalCounterpartyByEmail(IUnitOfWork uow, int emailId);
		/// <summary>
		/// Есть ли зарегистрированные пользователи физиков по этому номеру
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="phoneId">Идентификатор телефона</param>
		/// <returns></returns>
		bool HasExternalCounterparties(IUnitOfWork uow, int phoneId);
		/// <summary>
		/// Получения информации о внешних пользователях
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		IList<PersonalCounterpartyExternalUserInfo> GetPersonalCounterpartyExternalUsersInfo(IUnitOfWork uow, int counterpartyId);

		/// <summary>
		/// Есть ли у клиента действующий пользователь мобильного приложения.
		/// Сопоставление выполняется по неархивным телефонам клиента
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns><c>true</c>, если найден хотя бы один неархивный пользователь мобильного приложения</returns>
		bool HasActiveMobileAppUser(IUnitOfWork uow, int counterpartyId);
	}
}
