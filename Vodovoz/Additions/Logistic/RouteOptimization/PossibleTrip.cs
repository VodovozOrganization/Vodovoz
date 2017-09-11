using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class PossibleTrip
	{
		public AtWorkDriver Driver;

		public DeliveryShift Shift;

		public PossibleTrip(AtWorkDriver driver, DeliveryShift shift)
		{
			Driver = driver;
			Shift = shift;
		}
	}
}
