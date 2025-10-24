using System;
using QS.Project.Filter;

namespace Vodovoz.Filters.ViewModels
{
	public class BankAccountsMovementsJournalFilterViewModel  : FilterViewModelBase<BankAccountsMovementsJournalFilterViewModel>
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _onlyWithDiscrepancies;

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public bool OnlyWithDiscrepancies
		{
			get => _onlyWithDiscrepancies;
			set => UpdateFilterField(ref _onlyWithDiscrepancies, value);
		}
	}
}
