using QS.Views.GtkUI;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnTareReasonView : TabViewBase<ReturnTareReasonViewModel>
	{
		public ReturnTareReasonView(ReturnTareReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			ybtnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybtnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			yentryReturnTareReasonName.Binding.AddBinding(ViewModel.Entity, vm => vm.Name, w => w.Text).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
