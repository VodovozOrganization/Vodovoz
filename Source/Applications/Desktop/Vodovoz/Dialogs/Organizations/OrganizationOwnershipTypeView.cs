using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Dialogs.Organizations
{
	public partial class OrganizationOwnershipTypeView : TabViewBase<OrganizationOwnershipTypeViewModel>
	{
		public OrganizationOwnershipTypeView(OrganizationOwnershipTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			yentryAbbreviation.Binding.AddBinding(ViewModel.Entity, e => e.Abbreviation, w => w.Text).InitializeFromSource();
			yentryAbbreviation.Binding.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive).InitializeFromSource();

			ytextviewFullName.Binding.AddBinding(ViewModel.Entity, e => e.FullName, w => w.Buffer.Text).InitializeFromSource();
			ytextviewFullName.Binding.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive).InitializeFromSource();

			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive).InitializeFromSource();

			ybuttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybuttonSave.Binding.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive);

			ybuttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
