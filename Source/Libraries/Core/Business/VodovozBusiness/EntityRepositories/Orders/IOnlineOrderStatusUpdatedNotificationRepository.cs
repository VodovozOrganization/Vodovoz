using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderStatusUpdatedNotificationRepository
	{
		IEnumerable<OnlineOrderStatusUpdatedNotification> GetNotificationsForSend(
			IUnitOfWork uow, int days, int notificationCount);

		OnlineOrderNotificationSetting GetNotificationSetting(IUnitOfWork unitOfWork, ExternalOrderStatus externalOrderStatus);
		bool NeedCreateSendNotificationOfOnlineOrderStatusChanged(IUnitOfWork unitOfWork, OnlineOrder onlineOrder);
	}
}
