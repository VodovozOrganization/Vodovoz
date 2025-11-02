using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelLimitsManagementService
	{
		Task<IEnumerable<FuelLimit>> GetFuelLimitsByCardId(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<bool> RemoveFuelLimitById(string limitId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<IEnumerable<string>> SetFuelLimit(FuelLimit fuelLimit, string sessionId, string apiKey, CancellationToken cancellationToken);
	}
}
