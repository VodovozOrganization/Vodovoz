using QS.Views.Dialog;
using System;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoView : DialogViewBase<EdoViewModel>
	{
		public EdoView(EdoViewModel viewModel) : base(viewModel)
		{
			this.Build();

			edoinorderview.ViewModel = viewModel.EdoInOrderViewModel;
		}
	}
}
