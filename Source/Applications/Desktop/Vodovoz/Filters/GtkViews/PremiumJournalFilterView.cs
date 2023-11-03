using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	public partial class PremiumJournalFilterView : FilterViewBase<PremiumJournalFilterViewModel>
	{
		public PremiumJournalFilterView(PremiumJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			dateperiodpickerPremiumDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodpickerPremiumDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
		}
	}
}
