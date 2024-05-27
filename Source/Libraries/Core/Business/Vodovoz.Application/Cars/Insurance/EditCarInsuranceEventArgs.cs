using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class EditCarInsuranceEventArgs : EventArgs
	{
		public EditCarInsuranceEventArgs(CarInsurance selectedCarInsurance)
		{
			SelectedCarInsurance = selectedCarInsurance;
		}

		public CarInsurance SelectedCarInsurance { get; }
	}
}
