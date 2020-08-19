using System;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class FullInternalCallView : DialogViewBase<FullInternalCallViewModel>
	{
		public FullInternalCallView(FullInternalCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		public void Refresh()
		{
		}
	}
}
