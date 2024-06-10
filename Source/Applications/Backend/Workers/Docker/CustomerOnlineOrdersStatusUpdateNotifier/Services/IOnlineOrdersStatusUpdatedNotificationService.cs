using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public interface IOnlineOrdersStatusUpdatedNotificationService
	{
		Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source);
	}
}
