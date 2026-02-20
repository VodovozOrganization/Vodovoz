using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

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
		/// Есть ли зарегистрированные пользователи по этому номеру
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="phoneId">Идентификатор телефона</param>
		/// <returns></returns>
		bool HasExternalCounterparties(IUnitOfWork uow, int phoneId);
	}
}
