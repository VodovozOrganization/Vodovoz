using System;
using QS.Views.Dialog;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class IncomingCallView : DialogViewBase<IncomingCallViewModel>
	{
		public IncomingCallView(IncomingCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			labelName.Binding.AddBinding(viewModel.MangoManager, m => m.CallerName, w => w.Text).InitializeFromSource();
			labelNumber.Binding.AddFuncBinding<MangoManager>(viewModel.MangoManager, m => "Телефон: " + m.CallerNumber, w => w.Text).InitializeFromSource();
		}

		protected void OnButtonDisconnectClicked(object sender, EventArgs e)
		{
			ViewModel.DeclineCall();
		}
	}
}
