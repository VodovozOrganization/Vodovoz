using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrganizationOwnershipTypeJournalFilterView : FilterViewBase<OrganizationOwnershipTypeJournalFilterViewModel>
	{
		public OrganizationOwnershipTypeJournalFilterView(OrganizationOwnershipTypeJournalFilterViewModel organizationOwnershipTypeJournalFilterViewModel) : base(organizationOwnershipTypeJournalFilterViewModel)
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
