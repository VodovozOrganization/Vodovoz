using QS.Project.Filter;
using QS.Project.Services;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public partial class PayoutRequestJournalFilterViewModel : FilterViewModelBase<PayoutRequestJournalFilterViewModel>
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
		private PayoutDocumentsSortOrder _documentsSortOrder = PayoutDocumentsSortOrder.ByCreationDate;

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
				if(!SetField(ref _documentType, value))
				{
					return;
				}

				switch(value)
				{
					case PayoutRequestDocumentType.CashRequest:
						_canSetAccountable = true;
						_canSetCounterparty = false;
						_counterparty = null;
						OnPropertyChanged(nameof(CanSetAccountable));
						OnPropertyChanged(nameof(CanSetCounterparty));
						OnPropertyChanged(nameof(Counterparty));
						break;
					case PayoutRequestDocumentType.CashlessRequest:
						_canSetAccountable = false;
						_canSetCounterparty = true;
						_accountableEmployee = null;
						OnPropertyChanged(nameof(CanSetAccountable));
						OnPropertyChanged(nameof(CanSetCounterparty));
						OnPropertyChanged(nameof(AccountableEmployee));
						break;
					case null:
						_canSetAccountable = true;
						_canSetCounterparty = true;
						OnPropertyChanged(nameof(CanSetAccountable));
						OnPropertyChanged(nameof(CanSetCounterparty));
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
				Update();
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

		public virtual PayoutDocumentsSortOrder DocumentsSortOrder
		{
			get => _documentsSortOrder;
			set => UpdateFilterField(ref _documentsSortOrder, value);
		}

		public PayoutRequestJournalFilterViewModel(
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
			bool CheckRole(string roleName, int id) =>
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(roleName, id);

			var userId = ServicesConfig.CommonServices.UserService.CurrentUserId;

			if(CheckRole("role_financier_cash_request", userId))
			{
				return PayoutRequestUserRole.Financier;
			}

			if(CheckRole("role_coordinator_cash_request", userId))
			{
				return PayoutRequestUserRole.Coordinator;
			}

			if(CheckRole(Vodovoz.Permissions.Cash.RoleCashier, userId))
			{
				return PayoutRequestUserRole.Cashier;
			}

			if(CheckRole("role_cashless_payout_accountant", userId))
			{
				return PayoutRequestUserRole.Accountant;
			}
			
			if(CheckRole("role_security_service_cash_request", userId))
			{
				return PayoutRequestUserRole.SecurityService;
			}

			return PayoutRequestUserRole.Other;
		}
	}
}
