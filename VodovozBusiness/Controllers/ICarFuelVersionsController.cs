using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public interface ICarFuelVersionsController
	{
		void CreateAndAddVersion(double fuelConsumption, DateTime? startDate = null);
		void ChangeVersionStartDate(CarFuelVersion version, DateTime newStartDate);
		bool IsValidDateForNewCarVersion(DateTime dateTime);
		bool IsValidDateForVersionStartDateChange(CarFuelVersion version, DateTime newStartDate);
	}
}
