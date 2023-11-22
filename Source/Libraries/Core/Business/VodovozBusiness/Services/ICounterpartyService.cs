using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Services
{
	public interface ICounterpartyService
	{
		Task<IEnumerable<CounterpartyRevenueServiceInfo>> GetRevenueServiceInformation(
			string inn,
			string kpp,
			CancellationToken cancellationToken);
		Task StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, CancellationToken cancellationToken);
		void StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, bool isLiquidating, string statusName);
		Task StopShipmentsIfNeeded(int counterpartyId, int employeeId, CancellationToken cancellationToken);
		void UpdateDetailsFromRevenueServiceInfoIfNeeded(int counterpartyId, CounterpartyRevenueServiceInfo revenueServiceInfo);
	}
}
