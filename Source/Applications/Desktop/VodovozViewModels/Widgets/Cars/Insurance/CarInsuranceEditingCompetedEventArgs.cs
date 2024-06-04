using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceEditingCompetedEventArgs : EventArgs
	{
		public CarInsuranceEditingCompetedEventArgs(CarInsurance carInsurance)
		{
			CarInsurance = carInsurance;
		}

		public CarInsurance CarInsurance { get; }
	}
}
