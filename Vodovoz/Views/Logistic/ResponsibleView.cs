using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Complaints;

namespace Vodovoz.Views.Logistic
{
	public partial class ResponsibleView : TabViewBase<ResponsibleViewModel>
	{
		public ResponsibleView(ResponsibleViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			ycheckIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchived, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonSave.Sensitive = ViewModel.CanEdit;
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
