using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	public partial class PremiumJournalFilterView : FilterViewBase<PremiumJournalFilterViewModel>
	{
		public PremiumJournalFilterView(PremiumJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryreferenceSubdivisions.SubjectType = typeof(Subdivision);
			yentryreferenceSubdivisions.Binding.AddBinding(ViewModel, vm => vm.Subdivision, w => w.Subject).InitializeFromSource();
			
			dateperiodpickerPremiumDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodpickerPremiumDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
		}
	}
}
