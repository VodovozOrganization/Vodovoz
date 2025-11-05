using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class OnlineOrderStatusUpdatedNotificationRepository : IOnlineOrderStatusUpdatedNotificationRepository
	{
		public IEnumerable<OnlineOrderStatusUpdatedNotification> GetNotificationsForSend(
			IUnitOfWork uow, int days, int notificationCount)
		{
			var validCodes = new[] { 204, 200 };

			var notifications =
				from notification in uow.Session.Query<OnlineOrderStatusUpdatedNotification>()
				where (notification.HttpCode == null || !validCodes.Contains(notification.HttpCode.Value))
				      && notification.CreationDate >= DateTime.Today.AddDays(-days)
				orderby notification.HttpCode
				select notification;

			return notifications
				.Take(notificationCount)
				.ToList();
		}

		public OnlineOrderNotificationSetting GetNotificationSetting(IUnitOfWork unitOfWork, ExternalOrderStatus externalOrderStatus)
		{
			var notificationSetting = (from  notificationSettings in unitOfWork.Session.Query<OnlineOrderNotificationSetting>()
				where notificationSettings.ExternalOrderStatus == externalOrderStatus
				select notificationSettings)
				.SingleOrDefault();

			return notificationSetting;
		}

		public bool HasNotificationSentByOnlineOrder(IUnitOfWork unitOfWork, int onlineOrderId)
		{
			var hasNotificationSentByOnlineOrder = (from notification in unitOfWork.Session.Query<OnlineOrderStatusUpdatedNotification>()
					where notification.OnlineOrder.Id == onlineOrderId
					select notification.Id)
				.Any();

			return hasNotificationSentByOnlineOrder;
		}
	}
}
