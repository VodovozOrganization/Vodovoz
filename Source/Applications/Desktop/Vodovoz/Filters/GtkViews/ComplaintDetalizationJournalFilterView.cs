using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.Filters.GtkViews
{
	public partial class ComplaintDetalizationJournalFilterView : FilterViewBase<ComplaintDetalizationJournalFilterViewModel>
	{
		public ComplaintDetalizationJournalFilterView(ComplaintDetalizationJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjects, w => w.ItemsList)
				.AddBinding(vm => vm.ComplaintObject, w => w.SelectedItem)
				.InitializeFromSource();

			yspeccomboboxComplaintKind.ShowSpecialStateAll = true;
			yspeccomboboxComplaintKind.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleComplaintKinds, w => w.ItemsList)
				.AddBinding(vm => vm.ComplaintKind, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
