using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class EditCarOwnerEventArgs : EventArgs
	{
		public EditCarOwnerEventArgs(CarVersion carVersion)
		{
			CarVersion = carVersion;
		}

		public CarVersion CarVersion { get; }
	}
}
