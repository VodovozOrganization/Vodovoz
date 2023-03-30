using QS.Project.Filter;
using System;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark
{
	public class CashReceiptJournalFilterViewModel : FilterViewModelBase<CashReceiptJournalFilterViewModel>
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _hasUnscannedReason;
		private CashReceiptStatus? _status;
		

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

		public bool HasUnscannedReason
		{
			get => _hasUnscannedReason;
			set => UpdateFilterField(ref _hasUnscannedReason, value);
		}

		public CashReceiptStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}
	}
}
