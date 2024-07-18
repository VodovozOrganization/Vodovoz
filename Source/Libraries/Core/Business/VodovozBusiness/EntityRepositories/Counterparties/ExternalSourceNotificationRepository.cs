using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class ExternalSourceNotificationRepository : IExternalSourceNotificationRepository
	{
		public IEnumerable<ExternalCounterpartyAssignNotification> GetAssignedExternalCounterpartiesNotifications(IUnitOfWork uow, int days)
		{
			return from notification in uow.Session.Query<ExternalCounterpartyAssignNotification>()
				where (notification.HttpCode == null || (notification.HttpCode != 204 && notification.HttpCode != 0))
					&& notification.CreationDate >= DateTime.Today.AddDays(-days)
				select notification;
		}
		
		public IEnumerable<DeletedExternalCounterpartyNotification> GetDeletedExternalCounterpartiesNotifications(IUnitOfWork uow, int days)
		{
			return from notification in uow.Session.Query<DeletedExternalCounterpartyNotification>()
				where (notification.HttpCode == null || notification.HttpCode != 204)
					&& notification.CreationDate >= DateTime.Today.AddDays(-days)
				select notification;
		}
	}
}
