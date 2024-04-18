using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Services.Fuel
{
	public interface IFuelApiService
	{
		Task<(string SessionId, DateTime SessionExpirationDate)> Login(string login, string password, string apiKey, CancellationToken cancellationToken);
		Task<IEnumerable<FuelCard>> GetFuelCardsData(CancellationToken cancellationToken);
	}
}
