using System.Threading;
using System.Threading.Tasks;

namespace WarehouseApi.Library.Services
{
	public interface ILogisticsEventsCreationService
	{
		Task<bool> CreateEndLoadingWarehouseEvent(int documentId, CancellationToken cancellationToken);
		Task<bool> CreateStartLoadingWarehouseEvent(int documentId, CancellationToken cancellationToken);
	}
}
