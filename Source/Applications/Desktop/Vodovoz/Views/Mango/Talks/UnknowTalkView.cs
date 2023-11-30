using QS.Views.Dialog;
using System;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;

namespace Vodovoz.Views.Mango.Talks
{
	public partial class UnknowTalkView : DialogViewBase<UnknowTalkViewModel>
	{
		public UnknowTalkView(UnknowTalkViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			CallNumberLabel.Binding.AddBinding(ViewModel, v => v.PhoneText, w => w.LabelProp).InitializeFromSource();
		}
		#region Events
		protected void Clicked_NewClientButton(object sender, EventArgs e)
		{
			ViewModel.SelectNewConterparty();
		}

		protected void Clicked_ExistingClientButton(object sender, EventArgs e)
		{
			ViewModel.SelectExistConterparty();
		}

		protected void Clicked_ComplaintButton(object sender, EventArgs e)
		{
			ViewModel.CreateComplaintCommand();
		}

		protected void Clicked_StockBalnce(object sender, EventArgs e)
		{
			ViewModel.StockBalanceCommand();
		}

		protected void Clicked_CostAndDeliveryIntervalButton(object sender, EventArgs e)
		{
			ViewModel.CostAndDeliveryIntervalCommand();
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
		#endregion
	}
}
