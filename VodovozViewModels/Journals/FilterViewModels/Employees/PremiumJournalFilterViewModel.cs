using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Employees
{
	public class PremiumJournalFilterViewModel:FilterViewModelBase<PremiumJournalFilterViewModel>
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Subdivision _subdivision;

		public PremiumJournalFilterViewModel()
		{
			StartDate = EndDate = DateTime.Today;
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}
	}
}
