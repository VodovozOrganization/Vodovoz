using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class OnlineOrderStatusUpdatedNotificationRepository : IOnlineOrderStatusUpdatedNotificationRepository
	{
		public IEnumerable<OnlineOrderStatusUpdatedNotification> GetNotificationsForSend(IUnitOfWork uow, int days)
		{
			var notifications = from notification in uow.Session.Query<OnlineOrderStatusUpdatedNotification>()
								where (notification.HttpCode == null || notification.HttpCode != 204)
									&& notification.CreationDate >= DateTime.Today.AddDays(-days)
								select notification;

			return notifications.ToList();
		}
	}
}
