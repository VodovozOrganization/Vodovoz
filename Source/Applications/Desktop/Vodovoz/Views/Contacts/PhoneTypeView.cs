using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Views.Contacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhoneTypeView : TabViewBase<PhoneTypeViewModel>
	{
		public PhoneTypeView(PhoneTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive);
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			yenumcomboPurpose.ItemsEnum = typeof(PhonePurpose);
			yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.PhonePurpose, w => w.SelectedItem).InitializeFromSource();
			yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive).InitializeFromSource();
		}
	}
}
