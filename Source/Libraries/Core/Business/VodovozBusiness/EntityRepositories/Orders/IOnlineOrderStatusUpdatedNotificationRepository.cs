using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderStatusUpdatedNotificationRepository
	{
		IEnumerable<OnlineOrderStatusUpdatedNotification> GetNotificationsForSend(
			IUnitOfWork uow, int days, int notificationCount);
	}
}
