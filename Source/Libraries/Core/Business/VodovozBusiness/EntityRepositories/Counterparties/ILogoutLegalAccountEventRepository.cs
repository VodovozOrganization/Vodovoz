using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts.Events;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface ILogoutLegalAccountEventRepository
	{
		IEnumerable<LogoutLegalAccountEvent> GetLogoutLegalAccountEventsToSend(IUnitOfWork uow);
	}
}
