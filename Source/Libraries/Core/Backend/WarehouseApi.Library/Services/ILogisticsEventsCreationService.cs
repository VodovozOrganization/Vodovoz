using System.Threading;
using System.Threading.Tasks;

namespace WarehouseApi.Library.Services
{
	public interface ILogisticsEventsCreationService
	{
		Task<bool> CreateEndLoadingWarehouseEvent(int documentId, string accessToken, CancellationToken cancellationToken);
		Task<bool> CreateStartLoadingWarehouseEvent(int documentId, string accessToken, CancellationToken cancellationToken);
	}
}
