using QS.Views.GtkUI;

namespace Vodovoz.Dialogs.Client
{
	public partial class ClientCameFromView : TabViewBase<ClientCameFromViewModel>
	{
		public ClientCameFromView(ClientCameFromViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			entryName.Binding.AddBinding(ViewModel.Entity, x => x.Name, x => x.Text).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, x => x.IsArchive, x => x.Active).InitializeFromSource();
			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
