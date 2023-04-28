using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyAssignNotificationRepository
	{
		IList<ExternalCounterpartyAssignNotification> GetNotificationsForSend(IUnitOfWork uow, int days);
	}
}
