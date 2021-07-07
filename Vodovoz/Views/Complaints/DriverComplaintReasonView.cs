using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverComplaintReasonView : TabViewBase<DriverComplaintReasonViewModel>
	{
		public DriverComplaintReasonView(DriverComplaintReasonViewModel driverComplaintReasonViewModel)
			: base(driverComplaintReasonViewModel)
		{
			this.Build();
		}
	}
}
