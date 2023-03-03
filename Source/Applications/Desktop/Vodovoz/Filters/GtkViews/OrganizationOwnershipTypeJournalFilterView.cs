using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrganizationOwnershipTypeJournalFilterView : FilterViewBase<OrganizationOwnershipTypeJournalFilterViewModel>
	{
		public OrganizationOwnershipTypeJournalFilterView(OrganizationOwnershipTypeJournalFilterViewModel organizationOwnershipTypeJournalFilterViewModel) : base(сlientCameFromFilterViewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			yChkShowArchive.Binding.AddBinding(ViewModel, vm => vm.RestrictArchive, w => w.Active).InitializeFromSource();
		}
	}
}
