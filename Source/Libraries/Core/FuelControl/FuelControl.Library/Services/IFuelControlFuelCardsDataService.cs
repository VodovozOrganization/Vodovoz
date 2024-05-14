using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelControlFuelCardsDataService
	{
		Task<IEnumerable<FuelCard>> GetFuelCards(string sessionId, string apiKey, CancellationToken cancellationToken, int pageLimit = 500, int pageOffset = 0);
	}
}
