using System;
using QS.Project.Filter;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.ViewModels
{
	public class PaymentsJournalFilterViewModel : FilterViewModelBase<PaymentsJournalFilterViewModel>
	{
		public PaymentsJournalFilterViewModel()
		{
			StartDate = null;
			EndDate = null;
		}

		private DateTime? _startDate;
		private DateTime? _endDate;
		private PaymentState? _paymentState;
		private bool _hideCompleted;
		private bool _isManualCreated;

		public DateTime? StartDate {
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate {
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public PaymentState? PaymentState {
			get => _paymentState;
			set => UpdateFilterField(ref _paymentState, value);
		}

		public bool HideCompleted {
			get => _hideCompleted;
			set => UpdateFilterField(ref _hideCompleted, value);
		}
		
		public bool IsManualCreated
		{
			get => _isManualCreated;
			set => UpdateFilterField(ref _isManualCreated, value);
		}
	}
}
