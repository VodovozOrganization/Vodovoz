using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryVerificationDetailsView : DialogViewBase<FastDeliveryVerificationDetailsViewModel>
	{
		public FastDeliveryVerificationDetailsView(FastDeliveryVerificationDetailsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
