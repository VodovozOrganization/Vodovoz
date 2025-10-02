using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Tdi;
using System;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.ViewModels
{
	public partial class PaymentsJournalFilterViewModel : FilterViewModelBase<PaymentsJournalFilterViewModel>
	{
		private DateTime? _startDate = DateTime.Today.AddDays(-14);
		private DateTime? _endDate;
		private PaymentState? _paymentState;
		private bool _hideCompleted;
		private bool _hideCancelledPayments;
		private bool? _isManuallyCreated;
		private bool _hidePaymentsWithoutCounterparty;
		private bool _hideAllocatedPayments;
		private bool _isSortingDescByUnAllocatedSum;
		private Counterparty _counterparty;
		private Organization _organization;
		private PaymentJournalSortType _sortType;
		private Type _documentType;
		private bool _canChangeDocumentType;
		private bool _outgoingPaymentsWithoutCashlessRequestAssigned;

		public PaymentsJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			ITdiTab journalTab)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));
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
		
		public Organization Organization
		{
			get => _organization;
			set => UpdateFilterField(ref _organization, value);
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

		public bool? IsManuallyCreated
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

		public PaymentJournalSortType SortType
		{
			get => _sortType;
			set => UpdateFilterField(ref _sortType, value);
		}

		public object DocumentTypeObject
		{
			get => DocumentType;
			set
			{
				if(value is Type type)
				{
					DocumentType = type;
				}
				else
				{
					DocumentType = null;
				}
			}
		}

		public Type DocumentType
		{
			get => _documentType;
			set => UpdateFilterField(ref _documentType, value);
		}

		public override bool IsShow { get; set; } = true;

		public bool CanChangeDocumentType
		{
			get => _canChangeDocumentType;
			set => SetField(ref _canChangeDocumentType, value);
		}

		public Type RestrictDocumentType
		{
			get => CanChangeDocumentType ? null : DocumentType;
			set
			{
				if(value is null)
				{
					CanChangeDocumentType = true;
				}
				else
				{
					CanChangeDocumentType = false;
					DocumentType = value;
				}
			}
		}

		public bool OutgoingPaymentsWithoutCashlessRequestAssigned
		{
			get => _outgoingPaymentsWithoutCashlessRequestAssigned;
			set => UpdateFilterField(ref _outgoingPaymentsWithoutCashlessRequestAssigned, value);
		}
	}
}
