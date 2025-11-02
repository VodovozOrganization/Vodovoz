using System;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class DistrictSetDiffReportConfirmationViewModel
	{
		public class DistrictSetDiffReportConfirmationClosedArgs : EventArgs
		{
			public bool Canceled { get; set; }
		}
	}
}
