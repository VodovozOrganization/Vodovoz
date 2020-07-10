using System;

namespace Vodovoz.Services
{
	public interface IWageParametersProvider
	{
		int GetDaysWorkedForMinRatesLevel();
		decimal GetFixedWageForNewLargusDrivers();
		
		DateTime DontRecalculateWagesForRouteListsBefore { get; }
	}
}
