using System;
using System.Linq;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Tdi;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.ViewModels
{
	public class PaymentsJournalFilterViewModel : FilterViewModelBase<PaymentsJournalFilterViewModel>
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private PaymentState? _paymentState;
		private bool _hideCompleted;
		private bool _hideCancelledPayments;
		private bool _isManuallyCreated;
		private bool _hidePaymentsWithoutCounterparty;
		private bool _hideAllocatedPayments;
		private bool _isSortingDescByUnAllocatedSum;
		private Counterparty _counterparty;
		
		public PaymentsJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			ITdiTab journalTab,
			params Action<PaymentsJournalFilterViewModel>[] filterParams)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));

			Refilter(filterParams);
		}

		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public ITdiTab JournalTab { get; }

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

		public PaymentState? PaymentState
		{
			get => _paymentState;
			set => UpdateFilterField(ref _paymentState, value);
		}
		
		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public bool HideCompleted
		{
			get => _hideCompleted;
			set => UpdateFilterField(ref _hideCompleted, value);
		}

		public bool HideCancelledPayments
		{
			get => _hideCancelledPayments;
			set => UpdateFilterField(ref _hideCancelledPayments, value);
		}

		public bool IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => UpdateFilterField(ref _isManuallyCreated, value);
		}
		
		public bool HidePaymentsWithoutCounterparty
		{
			get => _hidePaymentsWithoutCounterparty;
			set => UpdateFilterField(ref _hidePaymentsWithoutCounterparty, value);
		}
		
		public bool HideAllocatedPayments
		{
			get => _hideAllocatedPayments;
			set => UpdateFilterField(ref _hideAllocatedPayments, value);
		}

		public bool IsSortingDescByUnAllocatedSum
		{
			get => _isSortingDescByUnAllocatedSum;
			set => UpdateFilterField(ref _isSortingDescByUnAllocatedSum, value);
		}
		
		public override bool IsShow { get; set; } = true;
		
		private void Refilter(Action<PaymentsJournalFilterViewModel>[] filterParams)
		{
			if(filterParams.Any())
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
	}
}
