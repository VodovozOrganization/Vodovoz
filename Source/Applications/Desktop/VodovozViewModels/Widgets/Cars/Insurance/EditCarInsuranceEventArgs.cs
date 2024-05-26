using System;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class EditCarInsuranceEventArgs : EventArgs
	{
		public EditCarInsuranceEventArgs(int selectedCarInsuranceId)
		{
			SelectedCarInsuranceId = selectedCarInsuranceId;
		}

		public int SelectedCarInsuranceId { get; }
	}
}
