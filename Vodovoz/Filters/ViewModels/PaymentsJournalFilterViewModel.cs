using System;
using QS.Project.Filter;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.ViewModels
{
	public class PaymentsJournalFilterViewModel : FilterViewModelBase<PaymentsJournalFilterViewModel>
	{
		private bool _hidePaymentsWithoutCounterparty;
		private Counterparty _counterparty;
		
		public PaymentsJournalFilterViewModel()
		{
			StartDate = null;
			EndDate = null;
		}
		
		public bool HidePaymentsWithoutCounterparty {
			get => _hidePaymentsWithoutCounterparty;
			set => UpdateFilterField(ref _hidePaymentsWithoutCounterparty, value);
		}

		private DateTime? _startDate;
		private DateTime? _endDate;
		private PaymentState? _paymentState;
		private bool _hideCompleted;
		private bool _isManuallyCreated;

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
		
		public Counterparty Counterparty {
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public bool HideCompleted {
			get => _hideCompleted;
			set => UpdateFilterField(ref _hideCompleted, value);
		}
		
		public bool IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => UpdateFilterField(ref _isManuallyCreated, value);
		}
	}
}
