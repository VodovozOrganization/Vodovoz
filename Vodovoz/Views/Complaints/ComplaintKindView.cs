using QS.Views.GtkUI;
using Vodovoz.ViewModels.Complaints;
using QS.Navigation;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintKindView : TabViewBase<ComplaintKindViewModel>
	{
		public ComplaintKindView(ComplaintKindViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
