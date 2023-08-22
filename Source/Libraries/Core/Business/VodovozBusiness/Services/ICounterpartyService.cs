using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Services
{
	public interface ICounterpartyService
	{
		Task StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, CancellationToken cancellationToken);
		Task StopShipmentsIfNeeded(int counterpartyId, int employeeId, CancellationToken cancellationToken);
	}
}
