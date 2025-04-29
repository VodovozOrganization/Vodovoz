using System;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Services.Fuel
{
	public interface IFuelCardVersionService
	{
		void ChangeVersionStartDate(FuelCardVersion version, DateTime newStartDate);
		void CreateAndAddVersion(FuelCard fuelCard, DateTime? startDate);
		bool IsValidDateForNewCarVersion(DateTime dateTime, FuelCard fuelCard);
		bool IsValidDateForVersionStartDateChange(FuelCardVersion version, DateTime newStartDate);
		bool IsDateTodayOrTomorow(DateTime date);
	}
}
