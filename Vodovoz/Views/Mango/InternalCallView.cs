using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class InternalCallView : DialogViewBase<InternalCallViewModel>
	{
		public InternalCallView(InternalCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected void OnComplaintButtonClicked(object sender, EventArgs e)
		{
			ViewModel.CreateComplaint();
		}
	}
}
