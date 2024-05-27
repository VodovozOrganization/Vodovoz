using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Services.Cars.Insurance
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
