using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Dialogs.Organizations
{
	public partial class OrganizationOwnershipTypeView : TabViewBase<OrganizationOwnershipTypeViewModel>
	{
		public OrganizationOwnershipTypeView(OrganizationOwnershipTypeViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			entryCode.Binding
				.AddBinding(ViewModel.Entity, e => e.Code, w => w.Text)
				.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();
			
			yentryAbbreviation.Binding
				.AddBinding(ViewModel.Entity, e => e.Abbreviation, w => w.Text)
				.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ytextviewFullName.Binding
				.AddBinding(ViewModel.Entity, e => e.FullName, w => w.Buffer.Text)
				.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yChkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, wm => wm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
