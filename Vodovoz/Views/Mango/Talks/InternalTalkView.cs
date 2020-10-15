using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango.Talks;

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
			LinePhone.Visible = labelOnLine.Visible = hboxInfo.Visible = ViewModel.IsTransfer;
			LinePhone.Binding.AddBinding(ViewModel, v => v.OnLine, l => l.LabelProp).InitializeFromSource();
			ForwardingButton.Visible = !ViewModel.IsTransfer;
			FinishButton.Label = ViewModel.IsTransfer ? "Вернуться к разговору" : "Завершить";

			CallNumberLabel.Text = ViewModel.GetCallerName();
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
