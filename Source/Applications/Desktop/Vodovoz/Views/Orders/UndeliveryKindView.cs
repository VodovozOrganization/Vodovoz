using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class UndeliveryKindView : TabViewBase<UndeliveryKindViewModel>
	{
		public UndeliveryKindView(UndeliveryKindViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yspeccomboboxUndeliveryObject.ShowSpecialStateNot = true;
			yspeccomboboxUndeliveryObject.Binding
				.AddBinding(ViewModel, vm => vm.UndeliveryObjects, w => w.ItemsList)
				.InitializeFromSource();
			yspeccomboboxUndeliveryObject.Binding
				.AddBinding(ViewModel.Entity, e => e.UndeliveryObject, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
