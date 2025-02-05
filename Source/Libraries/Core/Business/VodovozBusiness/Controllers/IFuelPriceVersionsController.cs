using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IFuelPriceVersionsController
	{
		void SetFuelType(FuelType fuelType);
		void CreateAndAddVersion(decimal fuelPrice, DateTime? startDate = null);
		void ChangeVersionStartDate(FuelPriceVersion version, DateTime newStartDate); 
		bool IsValidDateForNewCarVersion(DateTime dateTime);
		bool IsValidDateForVersionStartDateChange(FuelPriceVersion version, DateTime newStartDate);
	}
}
