using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	public partial class AutomaticallyAllocationBalanceWindowView : DialogViewBase<AutomaticallyAllocationBalanceWindowViewModel>
	{
		public AutomaticallyAllocationBalanceWindowView(AutomaticallyAllocationBalanceWindowViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
