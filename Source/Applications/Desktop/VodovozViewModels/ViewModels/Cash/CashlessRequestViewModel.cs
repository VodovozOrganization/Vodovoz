using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestViewModel : EntityTabViewModelBase<CashlessRequest>, IAskSaveOnCloseViewModel
	{
		private PayoutRequestUserRole _userRole;
		private readonly Employee _currentEmployee;
		private readonly IExpenseCategorySelectorFactory _expenseCategoryJournalFactory;
		private IEntityAutocompleteSelectorFactory _expenseCategoryAutocompleteSelectorFactory;

		public CashlessRequestViewModel(
			IFileDialogService fileDialogService,
			IExpenseCategorySelectorFactory expenseCategoryJournalFactory,
			IUserRepository userRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = base.TabName;
			CounterpartyAutocompleteSelector =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();
			_expenseCategoryJournalFactory =
				expenseCategoryJournalFactory ?? throw new ArgumentNullException(nameof(expenseCategoryJournalFactory));

			_currentEmployee =
				(employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository)))
				.GetEmployeeForCurrentUser(UoW);

			if(UoW.IsNew)
			{
				Entity.Author = _currentEmployee;
				Entity.Subdivision = _currentEmployee.Subdivision;
				Entity.Date = DateTime.Now;
				Entity.PayoutRequestState = PayoutRequestState.New;
			}

			UserRoles = GetUserRoles(CurrentUser.Id);
			IsRoleChooserSensitive = UserRoles.Count() > 1;
			UserRole = UserRoles.First();

			OurOrganisations = UoW.Session.QueryOver<Organization>().List();
			var filesViewModel = new CashlessRequestFilesViewModel(Entity, UoW, fileDialogService, CommonServices, userRepository)
			{
				ReadOnly = !IsNotClosed || IsSecurityServiceRole
			};
			CashlessRequestFilesViewModel = filesViewModel;

			ConfigureEntityChangingRelations();
		}

		#region Инициализация виджетов

		public CashlessRequestFilesViewModel CashlessRequestFilesViewModel { get; }
		public IEnumerable<Organization> OurOrganisations { get; }
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelector { get; }
		public IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory =>
			_expenseCategoryAutocompleteSelectorFactory
		    ?? (_expenseCategoryAutocompleteSelectorFactory =
				_expenseCategoryJournalFactory.CreateDefaultExpenseCategoryAutocompleteSelectorFactory());

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

		#endregion

		#region Настройки кнопок смены состояний

		public bool CanPayout => Entity.PayoutRequestState == PayoutRequestState.GivenForTake
		                         && UserRole == PayoutRequestUserRole.Accountant;

		public bool CanAccept => Entity.PayoutRequestState == PayoutRequestState.New
		                         || Entity.PayoutRequestState == PayoutRequestState.OnClarification;

		public bool CanApprove => Entity.PayoutRequestState == PayoutRequestState.Submited
		                          && UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanCancel => Entity.PayoutRequestState == PayoutRequestState.Submited
		                         || Entity.PayoutRequestState == PayoutRequestState.OnClarification
		                         || UserRole == PayoutRequestUserRole.Coordinator
		                         && (Entity.PayoutRequestState == PayoutRequestState.Agreed
		                             || Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		public bool CanReapprove => Entity.PayoutRequestState == PayoutRequestState.Agreed ||
		                            Entity.PayoutRequestState == PayoutRequestState.GivenForTake ||
		                            Entity.PayoutRequestState == PayoutRequestState.Canceled;

		public bool CanConveyForPayout => Entity.PayoutRequestState == PayoutRequestState.Agreed
		                                  && UserRole == PayoutRequestUserRole.Financier;

		#endregion

		#region Настройки остальных виджетов

		public bool IsRoleChooserSensitive { get; }
		public bool IsNotClosed => Entity.PayoutRequestState != PayoutRequestState.Closed;
		public bool IsNotNew => Entity.PayoutRequestState != PayoutRequestState.New;

		public bool CanSeeNotToReconcile => Entity.PayoutRequestState == PayoutRequestState.Submited
		                                    && UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanSeeOrganisation => UserRole == PayoutRequestUserRole.Financier;

		public bool CanSetOrganisaton => Entity.PayoutRequestState == PayoutRequestState.New
		                                 || Entity.PayoutRequestState == PayoutRequestState.Agreed
		                                 || Entity.PayoutRequestState == PayoutRequestState.GivenForTake;

		public bool CanSeeExpenseCategory => UserRole == PayoutRequestUserRole.Accountant
		                                     || UserRole == PayoutRequestUserRole.Financier;

		public bool CanSetExpenseCategory => Entity.PayoutRequestState == PayoutRequestState.New
		                                     || Entity.PayoutRequestState == PayoutRequestState.Agreed
		                                     || Entity.PayoutRequestState == PayoutRequestState.GivenForTake;

		public bool CanSetCancelReason => UserRole == PayoutRequestUserRole.Coordinator && IsNotClosed;

		public PayoutRequestUserRole UserRole
		{
			get => _userRole;
			set
			{
				SetField(ref _userRole, value);
				OnPropertyChanged(nameof(CanPayout));
				OnPropertyChanged(nameof(CanAccept));
				OnPropertyChanged(nameof(CanApprove));
				OnPropertyChanged(nameof(CanCancel));
				OnPropertyChanged(nameof(CanReapprove));
				OnPropertyChanged(nameof(CanConveyForPayout));
				OnPropertyChanged(nameof(CanSeeNotToReconcile));
				OnPropertyChanged(nameof(CanSeeOrganisation));
				OnPropertyChanged(nameof(CanSeeExpenseCategory));
				OnPropertyChanged(nameof(CanSetCancelReason));
			}
		}

		public bool IsSecurityServiceRole => UserRole == PayoutRequestUserRole.SecurityService;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => !IsSecurityServiceRole;

		#endregion

		#endregion

		#region Public методы

		public void Approve()
		{
			if(ValidateForNextState(PayoutRequestState.Agreed))
			{
				Save(true);
			}
		}

		public void Accept()
		{
			if(ValidateForNextState(PayoutRequestState.Submited))
			{
				Save(true);
			}
		}

		public void Cancel()
		{
			if(ValidateForNextState(PayoutRequestState.Canceled))
			{
				Save(true);
			}
		}

		public void Reapprove()
		{
			if(ValidateForNextState(PayoutRequestState.OnClarification))
			{
				Save(true);
			}
		}

		public void ConveyForPayout()
		{
			if(ValidateForNextState(PayoutRequestState.GivenForTake))
			{
				Save(true);
			}
		}

		public void Payout()
		{
			if(ValidateForNextState(PayoutRequestState.Closed))
			{
				Save(true);
			}
		}

		#endregion

		#region Private методы

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanAccept);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanApprove);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanCancel);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConveyForPayout);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanReapprove);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanPayout);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSetExpenseCategory);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSetOrganisaton);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSeeNotToReconcile);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => IsNotNew);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => IsNotClosed);
		}

		private bool ValidateForNextState(PayoutRequestState nextState)
		{
			ValidationContext.Items.Add("next_state", nextState);
			var valid = Validate();
			ValidationContext.Items.Remove("next_state");
			if(valid)
			{
				Entity.ChangeState(nextState);
			}

			return valid;
		}

		private IEnumerable<PayoutRequestUserRole> GetUserRoles(int userId)
		{
			bool CheckRole(string roleName, int id) =>
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(roleName, id);

			var roles = new List<PayoutRequestUserRole>();

			if(Entity.Author == null || Entity.Author.Id == _currentEmployee.Id)
			{
				roles.Add(PayoutRequestUserRole.RequestCreator);
			}

			if(CheckRole("role_financier_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Financier);
			}

			if(CheckRole("role_coordinator_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Coordinator);
			}

			if(CheckRole("role_cashless_payout_accountant", userId))
			{
				roles.Add(PayoutRequestUserRole.Accountant);
			}

			if(CheckRole("role_security_service_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.SecurityService);
			}

			if(roles.Count == 0)
			{
				throw new Exception("Пользователь не подходит ни под одну из ролей, он не должен был иметь возможность сюда зайти");
			}

			return roles;
		}

		#endregion
	}
}
