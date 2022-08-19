using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public interface IOdometerReadingsController
	{
		void CreateAndAddOdometerReading(DateTime? startDate = null);

		void ChangeOdometerReadingStartDate(OdometerReading version, DateTime newStartDate);

		bool IsValidDateForNewOdometerReading(DateTime dateTime);

		bool IsValidDateForOdometerReadingStartDateChange(OdometerReading version, DateTime newStartDate);
	}
}
