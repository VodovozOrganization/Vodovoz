using System;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public interface IFuelCardVersionController
	{
		void ChangeVersionStartDate(FuelCardVersion version, DateTime newStartDate);
		void CreateAndAddVersion(FuelCard fuelCard, DateTime? startDate);
		bool IsValidDateForNewCarVersion(DateTime dateTime, FuelCard fuelCard);
		bool IsValidDateForVersionStartDateChange(FuelCardVersion version, DateTime newStartDate);
	}
}
