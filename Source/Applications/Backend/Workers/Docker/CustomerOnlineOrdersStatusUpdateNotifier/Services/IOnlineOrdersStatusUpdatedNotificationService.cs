using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public interface IOnlineOrdersStatusUpdatedNotificationService
	{
		Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(
			OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source, CancellationToken cancellationToken = default);

		string GetPushText(IUnitOfWork unitOfWork, IOnlineOrderStatusUpdatedNotificationRepository notificationRepository,
			ExternalOrderStatus externalOrderStatus, int orderId, TimeSpan? deliveryScheduleFrom);
	}
}
