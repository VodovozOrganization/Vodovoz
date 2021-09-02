using System;
using QS.Project.Filter;
using QS.Project.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public class CashRequestJournalFilterViewModel : FilterViewModelBase<CashRequestJournalFilterViewModel>
	{
		private Employee _author;
		private Employee _accountableEmployee;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private PayoutRequestState? _state;
		private PayoutRequestDocumentType? _documentType;
		private Counterparty _counterparty;
		private bool _canSetAccountable = true;
		private bool _canSetCounterparty = true;

		public virtual Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public virtual Employee AccountableEmployee
		{
			get => _accountableEmployee;
			set => UpdateFilterField(ref _accountableEmployee, value);
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

		public virtual PayoutRequestState? State
		{
			get => _state;
			set => UpdateFilterField(ref _state, value);
		}

		public virtual PayoutRequestDocumentType? DocumentType
		{
			get => _documentType;
			set
			{
				if(!UpdateFilterField(ref _documentType, value))
				{
					return;
				}

				switch(value)
				{
					case PayoutRequestDocumentType.CashRequest:
						CanSetAccountable = true;
						CanSetCounterparty = false;
						Counterparty = null;
						break;
					case PayoutRequestDocumentType.CashlessRequest:
						CanSetAccountable = false;
						CanSetCounterparty = true;
						AccountableEmployee = null;
						break;
					case null:
						CanSetAccountable = true;
						CanSetCounterparty = true;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
			}
		}

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public virtual bool CanSetAccountable
		{
			get => _canSetAccountable;
			set => UpdateFilterField(ref _canSetAccountable, value);
		}

		public virtual bool CanSetCounterparty
		{
			get => _canSetCounterparty;
			set => UpdateFilterField(ref _canSetCounterparty, value);
		}

		public CashRequestJournalFilterViewModel(
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory)
		{
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
		}

		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }

		public PayoutRequestUserRole GetUserRole()
		{
			var userId = ServicesConfig.CommonServices.UserService.CurrentUserId;

			if(CashRequestViewModel.checkRole("role_financier_cash_request", userId))
			{
				return PayoutRequestUserRole.Financier;
			}

			if(CashRequestViewModel.checkRole("role_coordinator_cash_request", userId))
			{
				return PayoutRequestUserRole.Coordinator;
			}

			if(CashRequestViewModel.checkRole("role_сashier", userId))
			{
				return PayoutRequestUserRole.Cashier;
			}

			if(CashRequestViewModel.checkRole("role_cashless_payout_accountant", userId))
			{
				return PayoutRequestUserRole.Accountant;
			}

			return PayoutRequestUserRole.Other;
		}
	}
}
