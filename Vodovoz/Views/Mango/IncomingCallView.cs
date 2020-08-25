using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class IncomingCallView : DialogViewBase<IncomingCallViewModel>
	{
		public IncomingCallView(IncomingCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			CallNumberLabel.Text = ViewModel.Phone.Number;
		}

		protected void Clicked_ComplaintButton(object sender, EventArgs e)
		{
			ViewModel.CreateComplaint();
		}

		protected void Clicked_NewClientButton(object sender, EventArgs e)
		{
			ViewModel.SelectNewConterparty();
		}

		protected void Clicked_ExistingClientButton(object sender, EventArgs e)
		{
			ViewModel.SelectExistConterparty();
		}

		protected void Clicked_StockBalnce(object sender, EventArgs e)
		{
		}

		protected void Clicked_CostAndDeliveryIntervalButton(object sender, EventArgs e)
		{
		}

		protected void Clicked_FinishButton(object sender, EventArgs e)
		{
		}

		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
		}

		protected void Clicked_ForwardingToCoconsultationButton(object sender, EventArgs e)
		{
		}

		protected void Cliked_RollUpButton(object sender, EventArgs e)
		{
		}
	}
}
