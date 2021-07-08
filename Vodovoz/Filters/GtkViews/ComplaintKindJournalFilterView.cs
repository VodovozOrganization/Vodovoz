using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.Filters.GtkViews
{
	public partial class ComplaintKindJournalFilterView : FilterViewBase<ComplaintKindJournalFilterViewModel>
	{
		public ComplaintKindJournalFilterView(ComplaintKindJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddBinding(ViewModel, vm => vm.ComplaintObjects, w => w.ItemsList).InitializeFromSource();
			yspeccomboboxComplaintObject.Binding.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();
		}
	}
}