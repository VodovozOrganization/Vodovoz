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
			yentryAbbreviation.Binding.AddBinding(ViewModel.Entity, x => x.Abbreviation, x => x.Text).InitializeFromSource();
			ytextviewFullName.Binding.AddBinding(ViewModel.Entity, x => x.FullName, x => x.Buffer.Text).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, x => x.IsArchive, x => x.Active).InitializeFromSource();
			ybuttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybuttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
