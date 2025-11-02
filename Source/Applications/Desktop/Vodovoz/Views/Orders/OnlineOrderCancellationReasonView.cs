using System.ComponentModel;
using QS.Navigation;
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
			btnSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			
			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			
			lblId.Selectable = true;
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowId, w => w.Visible)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.InitializeFromSource();
			
			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
			
			entryReason.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
		}
	}
}
