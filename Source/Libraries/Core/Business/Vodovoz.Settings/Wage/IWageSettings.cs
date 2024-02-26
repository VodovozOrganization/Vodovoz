using System;

namespace Vodovoz.Services
{
	public interface IWageSettings
	{
		int DaysWorkedForMinRatesLevel { get; }
		DateTime DontRecalculateWagesForRouteListsBefore { get; }
		int SuburbWageDistrictId { get; }
		int CityWageDistrictId { get; }
	}
}
