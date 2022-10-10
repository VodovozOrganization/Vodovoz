using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.Filters.GtkViews
{
	public partial class ComplaintObjectJournalFilterView : FilterViewBase<ComplaintObjectJournalFilterViewModel>
	{
		public ComplaintObjectJournalFilterView(ComplaintObjectJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ydateperiodpickerCreateDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CreateDateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.CreateDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonArchive.Binding.AddBinding(ViewModel, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
