using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Employees
{
	public class PremiumJournalFilterViewModel:FilterViewModelBase<PremiumJournalFilterViewModel>
	{
		private DateTime? startDate;
		public virtual DateTime? StartDate
		{
			get => startDate;
			set => UpdateFilterField(ref startDate, value);
		}

		private DateTime? endDate;
		public virtual DateTime? EndDate
		{
			get => endDate;
			set => UpdateFilterField(ref endDate, value);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision
		{
			get => subdivision;
			set => UpdateFilterField(ref subdivision, value);
		}
	}
}
