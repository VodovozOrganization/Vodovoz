using Vodovoz.Domain.Contacts;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;

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

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };

			yenumcomboAdditionalType.ItemsEnum = typeof(PhoneAdditionalType);
			yenumcomboAdditionalType.Binding.AddBinding(ViewModel, e => e.PhoneAdditionalType, w => w.SelectedItem).InitializeFromSource();
		}
	}
}
