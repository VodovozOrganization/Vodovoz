using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public interface IOnlineOrdersStatusUpdatedNotificationService
	{
		Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(
			OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source, CancellationToken cancellationToken = default);
	}
}
