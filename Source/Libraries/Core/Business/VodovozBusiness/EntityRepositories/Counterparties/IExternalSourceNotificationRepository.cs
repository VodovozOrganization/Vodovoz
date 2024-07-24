using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalSourceNotificationRepository
	{
		IEnumerable<ExternalCounterpartyAssignNotification> GetAssignedExternalCounterpartiesNotifications(IUnitOfWork uow, int days);
		IEnumerable<DeletedExternalCounterpartyNotification> GetDeletedExternalCounterpartiesNotifications(IUnitOfWork uow, int days);
	}
}
