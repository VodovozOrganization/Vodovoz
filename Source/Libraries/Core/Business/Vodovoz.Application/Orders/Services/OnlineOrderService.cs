using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderService : IOnlineOrderService
	{
		private readonly IOnlineOrderStatusUpdatedNotificationRepository _onlineOrderStatusUpdatedNotificationRepository;

		public OnlineOrderService(IOnlineOrderStatusUpdatedNotificationRepository onlineOrderStatusUpdatedNotificationRepository)
		{
			_onlineOrderStatusUpdatedNotificationRepository =
				onlineOrderStatusUpdatedNotificationRepository ??
				throw new ArgumentNullException(nameof(onlineOrderStatusUpdatedNotificationRepository));
		}

		public void NotifyClientOfOnlineOrderStatusChange(IUnitOfWork unitOfWork, OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				return;
			}

			var hasBeenSent = _onlineOrderStatusUpdatedNotificationRepository.HasNotificationSentByOnlineOrder(unitOfWork, onlineOrder.Id);

			if(hasBeenSent)
			{
				return;
			}

			var notification = OnlineOrderStatusUpdatedNotification.Create(onlineOrder);

			unitOfWork.Save(notification);
		}
	}
}
