using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class ChangeStartDateEventArgs : EventArgs
	{
		public ChangeStartDateEventArgs(CarVersion carVersion, DateTime startDate)
		{
			CarVersion = carVersion;
			StartDate = startDate;
		}

		public CarVersion CarVersion { get; }
		public DateTime StartDate { get; }
	}
}
