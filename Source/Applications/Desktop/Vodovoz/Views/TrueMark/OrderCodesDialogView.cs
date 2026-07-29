using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz.Views.TrueMark
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderCodesDialogView : DialogViewBase<OrderCodesDialogViewModel>
	{
		public OrderCodesDialogView(OrderCodesDialogViewModel orderCodesDialogViewModel) 
		 : base(orderCodesDialogViewModel) 
		{
			this.Build();

			ViewModel.OrderCodesViewModel.ConfigureTransferTargetOrderEntry(ViewModel);
			ordercodesview.ViewModel = ViewModel.OrderCodesViewModel;
		}
	}
}
