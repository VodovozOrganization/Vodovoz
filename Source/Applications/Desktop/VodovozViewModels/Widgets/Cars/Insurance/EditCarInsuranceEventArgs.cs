using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class EditCarInsuranceEventArgs : EventArgs
	{
		public EditCarInsuranceEventArgs(CarInsurance carInsurance)
		{
			CarInsurance = carInsurance;
		}

		public CarInsurance CarInsurance { get; }
	}
}
