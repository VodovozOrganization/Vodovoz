using Vodovoz.Domain.Contacts;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;

namespace Vodovoz.Views.Contacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailTypeView : TabViewBase<EmailTypeViewModel>
	{
		public EmailTypeView(EmailTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };

			yenumcomboAdditionalType.ItemsEnum = typeof(EmailAdditionalType);
			yenumcomboAdditionalType.Binding.AddBinding(ViewModel, e => e.EmailAdditionalType, w => w.SelectedItem).InitializeFromSource();
		}
	}
}
