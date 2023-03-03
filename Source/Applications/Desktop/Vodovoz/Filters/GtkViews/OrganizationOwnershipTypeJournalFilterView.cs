using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrganizationOwnershipTypeJournalFilterView : FilterViewBase<OrganizationOwnershipTypeJournalFilterViewModel>
	{
		public OrganizationOwnershipTypeJournalFilterView(OrganizationOwnershipTypeJournalFilterViewModel organizationOwnershipTypeJournalFilterViewModel) : base(OrganizationOwnershipTypeViewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			yChkShowArchive.Binding.AddBinding(ViewModel, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
