using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelCardsGeneralInfoService
	{
		Task<IEnumerable<FuelCard>> GetFuelCards(string sessionId, string apiKey, int pageLimit = 500, int pageOffset = 0);
	}
}
