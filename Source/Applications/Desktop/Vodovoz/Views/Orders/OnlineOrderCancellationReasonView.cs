using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[ToolboxItem(true)]
	public partial class OnlineOrderCancellationReasonView : TabViewBase<OnlineOrderCancellationReasonViewModel>
	{
		public OnlineOrderCancellationReasonView(OnlineOrderCancellationReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
			
			entryReason.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
		}
	}
}
