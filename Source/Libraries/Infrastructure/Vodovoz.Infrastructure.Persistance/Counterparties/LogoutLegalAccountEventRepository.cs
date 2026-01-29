using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts.Events;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	public sealed class LogoutLegalAccountEventRepository : ILogoutLegalAccountEventRepository
	{
		public IEnumerable<LogoutLegalAccountEvent> GetLogoutLegalAccountEventsToSend(IUnitOfWork uow)
		{
			return (from eventData in uow.Session.Query<LogoutLegalAccountEventSourceSentData>()
				join @event in uow.Session.Query<LogoutLegalAccountEvent>()
					on eventData.Event.Id equals @event.Id
				where eventData.Delivered == false
				select @event)
				.ToList();
		}
	}
}
