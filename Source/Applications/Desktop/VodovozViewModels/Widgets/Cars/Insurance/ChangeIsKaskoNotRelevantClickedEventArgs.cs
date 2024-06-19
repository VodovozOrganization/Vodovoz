using System;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class ChangeIsKaskoNotRelevantClickedEventArgs : EventArgs
	{
		public ChangeIsKaskoNotRelevantClickedEventArgs(bool isKaskoNotRelevant)
		{
			IsKaskoNotRelevant = isKaskoNotRelevant;
		}

		public bool IsKaskoNotRelevant { get; }
	}
}
