using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class AddCarInsuranceEventArgs : EventArgs
	{
		public AddCarInsuranceEventArgs(CarInsuranceType insuranceType)
		{
			InsuranceType = insuranceType;
		}

		public CarInsuranceType InsuranceType { get; }
	}
}
