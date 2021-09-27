using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class UndeliveryTransferAbsenceReasonView : TabViewBase<UndeliveryTransferAbsenceReasonViewModel>
	{
		public UndeliveryTransferAbsenceReasonView(UndeliveryTransferAbsenceReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ychkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
