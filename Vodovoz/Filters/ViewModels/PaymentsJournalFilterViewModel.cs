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

		private DateTime? startDate;
		public DateTime? StartDate {
			get => startDate;
			set => UpdateFilterField(ref startDate, value);
		}

		private DateTime? endDate;
		public DateTime? EndDate {
			get => endDate;
			set => UpdateFilterField(ref endDate, value);
		}

		private PaymentState? paymentState;
		public PaymentState? PaymentState {
			get => paymentState;
			set => UpdateFilterField(ref paymentState, value);
		}
		
		public Counterparty Counterparty {
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		bool hideCompleted;
		public bool HideCompleted {
			get => hideCompleted;
			set => UpdateFilterField(ref hideCompleted, value);
		}
	}
}
