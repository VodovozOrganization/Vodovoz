using System;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class AddNewVersionEventArgs : EventArgs
	{
		public AddNewVersionEventArgs(DateTime startDateTime)
		{
			StartDateTime = startDateTime;
		}

		public DateTime StartDateTime { get; }
	}
}
