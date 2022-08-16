using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IFuelPriceVersionsController
	{
		void CreateAndAddVersion(decimal fuelPrice, DateTime? startDate = null);
		void ChangeVersionStartDate(FuelPriceVersion version, DateTime newStartDate);
	}
}
