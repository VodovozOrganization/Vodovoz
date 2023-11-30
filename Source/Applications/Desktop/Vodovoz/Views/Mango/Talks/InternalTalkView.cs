using QS.Views.Dialog;
using System;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;

namespace Vodovoz.Views.Mango.Talks
{
	public partial class InternalTalkView : DialogViewBase<InternalTalkViewModel>
	{
		public InternalTalkView(InternalTalkViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			CallNumberLabel.Binding.AddBinding(ViewModel, v => v.CallerNameText, w => w.LabelProp).InitializeFromSource();
			LinePhone.Visible = labelOnLine.Visible = ViewModel.ShowTransferCaller;
			LinePhone.Binding.AddBinding(ViewModel, v => v.OnLineText, l => l.LabelProp).InitializeFromSource();
			ForwardingButton.Visible = ViewModel.ShowTransferButton;
			hboxInfo.Visible = ViewModel.ShowReturnButton;
			FinishButton.Label = ViewModel.ShowReturnButton ? "Вернуться к разговору" : "Завершить";
		}

		#region MangoEvents
		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			ViewModel.ForwardCallCommand();
		}

		protected void Clicked_FinishButton(object sender, EventArgs e)
		{
			ViewModel.FinishCallCommand();
		}

		#endregion
	}
}
