using System;

namespace Vodovoz.Settings.Fuel
{
	public interface IFuelControlSettings
	{
		string ApiBaseAddress { get; }
		string ApiSessionLifetimeDays { get; }
		TimeSpan ApiRequesTimeout { get; }
		string OrganizationContractId { get; }
		DateTime FuelTransactionsPerDayLastUpdateDate { get; }
		DateTime FuelTransactionsPerMonthLastUpdateDate { get; }
		void SetFuelTransactionsPerDayLastUpdateDate(string value);
		void SetFuelTransactionsPerMonthLastUpdateDate(string value);
	}
}
