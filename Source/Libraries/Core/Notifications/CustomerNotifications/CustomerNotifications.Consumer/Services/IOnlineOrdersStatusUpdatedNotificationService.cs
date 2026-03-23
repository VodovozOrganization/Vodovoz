using CustomerNotifications.Consumer.Contracts;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Orders;

namespace CustomerNotifications.Consumer.Services
{
	public interface IOnlineOrdersStatusUpdatedNotificationService
	{
		Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(
			OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source, CancellationToken cancellationToken = default);

		string GetPushText(IUnitOfWork unitOfWork, IOnlineOrderNotificationSettingsProvider onlineOrderNotificationSettingsProvider, CustomerNotificationMessage message, OnlineOrder onlineOrder);
	}
}
