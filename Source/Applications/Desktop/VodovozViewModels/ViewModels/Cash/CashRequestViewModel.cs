using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.NotificationSenders;
using Vodovoz.Tools;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;
using static Vodovoz.ViewModels.ViewModels.Cash.CashRequestItemViewModel;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashRequestViewModel : EntityTabViewModelBase<CashRequest>, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICashRepository _cashRepository;
		private readonly HashSet<CashRequestSumItem> _sumsGiven = new HashSet<CashRequestSumItem>();
		private readonly ICashRequestForDriverIsGivenForTakeNotificationSender _cashRequestForDriverIsGivenForTakeNotificationSender;
		private FinancialExpenseCategory _financialExpenseCategory;

		private CashRequestSumItem _selectedItem;
		private CashRequestSumItem _selectedCashRequestSumItem;
		private decimal _sumForPartiallyGive;

		#region Статья расхода

		private bool _hasFinancialExpenseCategoryPermission => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.FinancialCategory.CanChangeFinancialExpenseCategory);

		private PayoutRequestState[] _expenseCategoriesForAll => new[]
		{
			PayoutRequestState.New,
			PayoutRequestState.OnClarification,
			PayoutRequestState.Submited
		};

		private PayoutRequestState[] _expenseCategoriesWithSpecialPermission => new[]
		{
			PayoutRequestState.Agreed,
			PayoutRequestState.GivenForTake,
			PayoutRequestState.PartiallyClosed
		};

		#endregion

		public CashRequestViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			ICashRepository cashRepository,
			INavigationManager navigation,
			ICashRequestForDriverIsGivenForTakeNotificationSender cashRequestForDriverIsGivenForTakeNotificationSender,
			ViewModelEEVMBuilder<Employee> authorViewModelBuilder,
			ViewModelEEVMBuilder<Subdivision> subdivisionViewModelBuilder,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(uowBuilder is null)
			{
				throw new ArgumentNullException(nameof(uowBuilder));
			}

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			if(authorViewModelBuilder is null)
			{
				throw new ArgumentNullException(nameof(authorViewModelBuilder));
			}

			if(subdivisionViewModelBuilder is null)
			{
				throw new ArgumentNullException(nameof(subdivisionViewModelBuilder));
			}

			if(financialExpenseCategoryViewModelBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialExpenseCategoryViewModelBuilder));
			}

			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_cashRepository = cashRepository
				?? throw new ArgumentNullException(nameof(cashRepository));
			_cashRequestForDriverIsGivenForTakeNotificationSender = cashRequestForDriverIsGivenForTakeNotificationSender
				?? throw new ArgumentNullException(nameof(cashRequestForDriverIsGivenForTakeNotificationSender));

			IsNewEntity = uowBuilder.IsNewEntity;
			CurrentEmployee = employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(Entity.Subdivision?.FinancialResponsibilityCenterId != null)
			{
				FinancialResponsibilityCenter = UoW.GetById<FinancialResponsibilityCenter>(Entity.Subdivision.FinancialResponsibilityCenterId.Value);
			}

			if(IsNewEntity)
			{
				Entity.Author = CurrentEmployee;
			}

			Entity.Subdivision = CurrentEmployee.Subdivision;

			TabName = IsNewEntity ? "Создание новой заявки на выдачу ДС" : $"{Entity.Title}";

			UserRoles = GetUserRoles(CurrentUser.Id);

			if(!UserRoles.Any())
			{
				Dispose();
				throw new AbortCreatingPageException("Нет прав для открытия диалога", "Невозможно открыть");
			}

			IsRoleChooserSensitive = UserRoles.Count() > 1;
			UserRole = UserRoles.First();

			ConfigureEntityChangingRelations();

			#region ViewModels

			AuthorViewModel = authorViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Author)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			SubdivisionViewModel = subdivisionViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			FinancialExpenseCategoryViewModel = financialExpenseCategoryViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FinancialExpenseCategory)
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			#endregion ViewModels

			AcceptCommand = new DelegateCommand(AcceptCommandHandler, () => true);

			SubdivisionChiefApproveCommand = new DelegateCommand(SubdivisionChiefApproveCommandHandler,
				() => CanSubdivisionChiefApprove);
			SubdivisionChiefApproveCommand.CanExecuteChangedWith(this, x => x.CanSubdivisionChiefApprove);

			AgreeByFinancialResponsibilityCenterCommand = new DelegateCommand(AgreeByFinancialResponsibilityCenterCommandHandler, () => CanAgreeByFinancialResponsibilityCenter);
			AgreeByFinancialResponsibilityCenterCommand.CanExecuteChangedWith(this, x => x.CanAgreeByFinancialResponsibilityCenter);

			ApproveCommand = new DelegateCommand(Approve, () => true);

			CloseCommand = new DelegateCommand(Close, () => true);

			CancelRequestCommand = new DelegateCommand(CancelRequestCommandHandler, () => true);

			ConveyForResultsCommand = new DelegateCommand(ConveyForResults, () => true);

			ReturnToRenegotiationCommand = new DelegateCommand(ReturnToRenegotiation, () => true);

			AddSumCommand = new DelegateCommand(AddSum, () => true);

			EditSumCommand = new DelegateCommand(EditSum, () => true);

			RemoveSumCommand = new DelegateCommand(RemoveSum, () => true);

			AfterSaveCommand = new DelegateCommand(AfterSaveHandler, () => true);

			GiveSumCommand = new DelegateCommand(GiveSum, () => CanExecuteGive);
			GiveSumCommand.CanExecuteChangedWith(this, x => x.CanExecuteGive);

			GiveSumPartiallyCommand = new DelegateCommand(GiveSumPartially, () => CanExecuteGive);
			GiveSumPartiallyCommand.CanExecuteChangedWith(this, x => x.CanExecuteGive);

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		private void Close()
		{
			Close(AskSaveOnClose, CloseSource.Cancel);
		}

		private void AfterSaveHandler()
		{
			if(Entity.ExpenseCategoryId == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Необходимо заполнить статью расхода");
				return;
			}

			var entityId = Entity.Id;
			var entityStatusIsGivenForTake = Entity.PayoutRequestState == PayoutRequestState.GivenForTake;

			SaveAndClose();

			if(entityStatusIsGivenForTake)
			{
				_cashRequestForDriverIsGivenForTakeNotificationSender
					.NotifyOfCashRequestForDriverIsGivenForTake(entityId);
			}

			if(AfterSave(out var messageText))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					$"Cоздан следующие аванс:\n{messageText}");
			}
		}

		private void Approve()
		{
			ChangeStateAndSave(PayoutRequestState.Agreed);
		}

		private void RemoveSum()
		{
			if(Entity.ObservableSums.Contains(SelectedItem))
			{
				Entity.ObservableSums.Remove(SelectedItem);
			}
		}

		private void EditSum()
		{
			var cashRequestItemPage = NavigationManager
				.OpenViewModel<CashRequestItemViewModel, IUnitOfWork, PayoutRequestUserRole>(this, UoW, UserRole, OpenPageOptions.AsSlave);

			cashRequestItemPage.ViewModel.Entity = SelectedItem;
		}

		private void AddSum()
		{
			var cashRequestItemPage = NavigationManager
				.OpenViewModel<CashRequestItemViewModel, IUnitOfWork, PayoutRequestUserRole>(this, UoW, UserRole, OpenPageOptions.AsSlave);

			cashRequestItemPage.ViewModel.Entity = new CashRequestSumItem()
			{
				AccountableEmployee = CurrentEmployee
			};

			cashRequestItemPage.ViewModel.EntityAccepted += OnSumForAddAccepted;
		}

		private void OnSumForAddAccepted(object sender, EventArgs args)
		{
			if(args is CashRequestSumItemAcceptedEventArgs acceptedArgs)
			{
				Entity.AddItem(acceptedArgs.AcceptedEntity);
				acceptedArgs.AcceptedEntity.CashRequest = Entity;
			}

			if(sender is CashRequestItemViewModel cashRequestItemViewModel)
			{
				cashRequestItemViewModel.EntityAccepted -= OnSumForAddAccepted;
			}
		}

		private void ReturnToRenegotiation()
		{
			if(string.IsNullOrEmpty(Entity.ReasonForSendToReappropriate))
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Причина отправки на пересогласование должна быть заполнена"
				);
			}
			else
			{
				ChangeStateAndSave(PayoutRequestState.OnClarification);
			}
		}

		private void ConveyForResults()
		{
			if(Entity.PayoutRequestState == PayoutRequestState.Agreed
				&& Entity.ExpenseCategoryId == null
				&& UserRole == PayoutRequestUserRole.Cashier)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Необходимо заполнить статью расхода");
			}
			else
			{
				ChangeStateAndSave(PayoutRequestState.GivenForTake);
			}
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(CashRequest.Subdivision))
			{
				if(Entity.Subdivision?.FinancialResponsibilityCenterId == null)
				{
					FinancialResponsibilityCenter = null;
					return;
				}

				FinancialResponsibilityCenter = UoW.GetById<FinancialResponsibilityCenter>(Entity.Subdivision.FinancialResponsibilityCenterId.Value);
			}
		}

		private void AgreeByFinancialResponsibilityCenterCommandHandler()
		{
			ChangeStateAndSave(PayoutRequestState.AgreedByFinancialResponsibilityCenter);
		}

		[PropertyChangedAlso(nameof(CanExecuteGive))]
		public object SelectedCashRequestSumItemObject
		{
			get => SelectedCashRequestSumItem;
			set
			{
				if(value is CashRequestSumItem sumItem)
				{
					SelectedCashRequestSumItem = sumItem;
				}
				else
				{
					SelectedCashRequestSumItem = null;
				}
				OnPropertyChanged(nameof(SelectedCashRequestSumItemObject));
			}
		}

		[PropertyChangedAlso(nameof(SelectedCashRequestSumItemObject))]
		public CashRequestSumItem SelectedCashRequestSumItem
		{
			get => _selectedCashRequestSumItem;
			set => _selectedCashRequestSumItem = value;
		}

		public decimal SumForPartiallyGive
		{
			get => _sumForPartiallyGive;
			set => SetField(ref _sumForPartiallyGive, value);
		}

		public Employee CurrentEmployee { get; }

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		public FinancialResponsibilityCenter FinancialResponsibilityCenter { get; private set; }

		#region ViewModels

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		public IEntityEntryViewModel SubdivisionViewModel { get; }

		public IEntityEntryViewModel AuthorViewModel { get; }

		#endregion ViewModels

		#region Commands

		public DelegateCommand AddSumCommand { get; }

		public DelegateCommand EditSumCommand { get; }

		public DelegateCommand RemoveSumCommand { get; }

		public DelegateCommand AfterSaveCommand { get; }

		public DelegateCommand GiveSumCommand { get; }

		public DelegateCommand GiveSumPartiallyCommand { get; }

		public DelegateCommand AcceptCommand { get; }

		public DelegateCommand SubdivisionChiefApproveCommand { get; }

		public DelegateCommand AgreeByFinancialResponsibilityCenterCommand { get; }

		public DelegateCommand ApproveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		public DelegateCommand CancelRequestCommand { get; }

		public DelegateCommand ConveyForResultsCommand { get; }

		public DelegateCommand ReturnToRenegotiationCommand { get; }

		private string AuthorsSubdivisionChiefName =>
			Entity.Author?.Subdivision?.Chief?.ShortName ?? "Руководитель не указан";

		public string StateName =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			? $"{Entity.PayoutRequestState.GetEnumTitle()}. Ожидает согласования {AuthorsSubdivisionChiefName}"
			: Entity.PayoutRequestState.GetEnumTitle();

		// TODO: оповещения об изменении
		public bool CanExecuteGive =>
			Entity.PayoutRequestState != PayoutRequestState.Closed
			&& !IsSecurityServiceRole
			&& SelectedCashRequestSumItem != null
			&& SelectedCashRequestSumItem.Sum > SelectedCashRequestSumItem.Expenses
				.Sum(e => e.Money)
			&& (Entity.PossibilityNotToReconcilePayments
				|| SelectedCashRequestSumItem.Expenses.Any()
				|| Entity.ObservableSums.All(x => !x.Expenses.Any()
				|| x.Sum == x.Expenses.Sum(e => e.Money)));

		private void CancelRequestCommandHandler()
		{
			if(string.IsNullOrEmpty(Entity.CancelReason)
			&& UserRole == PayoutRequestUserRole.Coordinator)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Причина отмены должна быть заполнена");
			}
			else
			{
				ChangeStateAndSave(PayoutRequestState.Canceled);
			}
		}

		private void AcceptCommandHandler()
		{
			ChangeStateAndSave(PayoutRequestState.Submited);
			ShowInfoMessage($"Ваша заявка передана на согласование {AuthorsSubdivisionChiefName}");
		}

		private void SubdivisionChiefApproveCommandHandler()
		{
			ChangeStateAndSave(PayoutRequestState.AgreedBySubdivisionChief);
		}

		private void GiveSum()
		{
			GiveSum(SelectedCashRequestSumItem);
		}

		private void GiveSumPartially()
		{
			GiveSum(SelectedCashRequestSumItem, SumForPartiallyGive);
		}

		#endregion Commands

		#region Properties

		private PayoutRequestUserRole _userRole;

		[PropertyChangedAlso(
			nameof(CanEditOnlyCoordinator),
			nameof(SensitiveForFinancier),
			nameof(ExpenseCategorySensitive),
			nameof(VisibleOnlyForFinancer),
			nameof(CanSeeGiveSum),
			nameof(CanGiveSum),
			nameof(CanSubdivisionChiefApprove),
			nameof(CanApprove),
			nameof(CanConveyForResults),
			nameof(CanReturnToRenegotiation),
			nameof(CanCancel),
			nameof(CanConfirmPossibilityNotToReconcilePayments),
			nameof(ExpenseCategoryVisibility),
			nameof(CanAddSum),
			nameof(IsSecurityServiceRole))]
		public PayoutRequestUserRole UserRole
		{
			get => _userRole;
			set => SetField(ref _userRole, value);
		}

		public bool IsNewEntity { get; private set; }
		public bool IsRoleChooserSensitive { get; set; }

		[PropertyChangedAlso(nameof(CanEditSumSensitive))]
		public CashRequestSumItem SelectedItem
		{
			get => _selectedItem;
			set => SetField(ref _selectedItem, value);
		}

		#region Editability

		public bool CanEditOnlyCoordinator => UserRole == PayoutRequestUserRole.Coordinator;

		public bool SensitiveForFinancier =>
			UserRole == PayoutRequestUserRole.Financier
			&& (Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.Agreed
			|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		public bool ExpenseCategorySensitive =>
			_expenseCategoriesForAll.Contains(Entity.PayoutRequestState)
			|| (_expenseCategoriesWithSpecialPermission.Contains(Entity.PayoutRequestState)
				&& _hasFinancialExpenseCategoryPermission);

		//редактировать можно только не выданные
		public bool CanEditSumSensitive =>
			SelectedItem != null
			&& !SelectedItem.ObservableExpenses.Any();

		#endregion Editability

		#region Visibility

		public bool VisibleOnlyForFinancer => UserRole == PayoutRequestUserRole.Financier;

		public bool VisibleOnlyForStatusUpperThanCreated => Entity.PayoutRequestState != PayoutRequestState.New;

		public bool ExpenseCategoryVisibility => true;

		public bool CanConfirmPossibilityNotToReconcilePayments =>
			Entity.ObservableSums.Count > 1
			&& Entity.PayoutRequestState == PayoutRequestState.Submited
			&& UserRole == PayoutRequestUserRole.Coordinator;

		#endregion Visibility

		#region Permissions

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanSeeGiveSum =>
			UserRole == PayoutRequestUserRole.Cashier
			&& (Entity.PayoutRequestState == PayoutRequestState.GivenForTake
			|| Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed);

		public bool CanGiveSum =>
			UserRole == PayoutRequestUserRole.Cashier
			&& (Entity.PayoutRequestState == PayoutRequestState.GivenForTake
				|| Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed);

		public bool CanDeleteSum => IsNewEntity;

		//Подтвердить
		public bool CanAccept =>
			Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.OnClarification;

		//Согласовать руководителем отдела
		public bool CanSubdivisionChiefApprove =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			&& UserRole == PayoutRequestUserRole.SubdivisionChief;

		//Согласование ЦФО
		public bool CanAgreeByFinancialResponsibilityCenter =>
			Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
			&& (CurrentEmployee.Id == FinancialResponsibilityCenter?.ResponsibleEmployeeId
				|| CurrentEmployee.Id == FinancialResponsibilityCenter?.ViceResponsibleEmployeeId);

		//Согласовать исполнительным директором
		public bool CanApprove =>
			(Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
				|| Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief // Убрать по заполнении ЦФО
				|| Entity.PayoutRequestState == PayoutRequestState.Submited)
			&& UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanConveyForResults =>
			UserRole == PayoutRequestUserRole.Financier
			&& Entity.PayoutRequestState == PayoutRequestState.Agreed;

		public bool CanReturnToRenegotiation =>
			(Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
				|| Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
				|| Entity.PayoutRequestState == PayoutRequestState.Agreed
				|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake
				|| Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed)
			&& UserRole != PayoutRequestUserRole.RequestCreator
			|| Entity.PayoutRequestState == PayoutRequestState.Canceled;

		public bool CanCancel =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			|| Entity.PayoutRequestState == PayoutRequestState.OnClarification
			|| ((Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
					|| Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
					|| Entity.PayoutRequestState == PayoutRequestState.Agreed)
				&& (UserRole == PayoutRequestUserRole.SubdivisionChief
					|| UserRole == PayoutRequestUserRole.Coordinator))
			|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake
			&& UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanAddSum =>
			(Entity.PayoutRequestState == PayoutRequestState.New
				|| Entity.PayoutRequestState == PayoutRequestState.OnClarification
				|| Entity.PayoutRequestState == PayoutRequestState.Submited
				|| UserRole == PayoutRequestUserRole.Coordinator)
			&& !IsSecurityServiceRole;

		#endregion Permissions

		public bool IsSecurityServiceRole => UserRole == PayoutRequestUserRole.SecurityService;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => !IsSecurityServiceRole;

		#endregion

		#endregion Properties

		private void GiveSum(CashRequestSumItem sumItem, decimal? sumToGive = null)
		{
			if(!Entity.Sums.Any())
			{
				return;
			}

			if(Entity.ExpenseCategoryId == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"У данной заявки не заполнена статья расхода");
				return;
			}

			var cashRequestSumItem = sumItem
				?? Entity.ObservableSums.FirstOrDefault(x => !x.ObservableExpenses.Any());

			if(cashRequestSumItem == null)
			{
				return;
			}

			var alreadyGiven = cashRequestSumItem.ObservableExpenses.Sum(x => x.Money);

			var decimalSumToGive = sumToGive ?? cashRequestSumItem.Sum - alreadyGiven;

			if(decimalSumToGive <= 0)
			{
				return;
			}

			CreateNewExpenseForItem(cashRequestSumItem, decimalSumToGive);

			if(!Entity.PossibilityNotToReconcilePayments
				&& alreadyGiven > 0
				&& alreadyGiven + decimalSumToGive == cashRequestSumItem.Sum
				&& Entity.ObservableSums.Count(x => x.Expenses.Sum(e => e.Money) != x.Sum) > 0)
			{
				ChangeStateAndSave(PayoutRequestState.OnClarification);
			}
			else
			{
				ChangeStateAndSave(PayoutRequestState.Closed);
			}
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.PayoutRequestState,
				() => StateName,
				() => CanEditOnlyCoordinator,
				() => SensitiveForFinancier,
				() => ExpenseCategorySensitive,
				() => CanEditSumSensitive,
				() => VisibleOnlyForStatusUpperThanCreated,
				() => CanSeeGiveSum,
				() => CanAccept,
				() => CanSubdivisionChiefApprove,
				() => CanApprove,
				() => CanConveyForResults,
				() => CanReturnToRenegotiation,
				() => CanCancel,
				() => CanConfirmPossibilityNotToReconcilePayments);

			SetPropertyChangeRelation(e => e.Subdivision,
				() => CanAgreeByFinancialResponsibilityCenter);

			SetPropertyChangeRelation(e => e.ObservableSums, () => CanGiveSum);

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);
		}

		private IEnumerable<PayoutRequestUserRole> GetUserRoles(int userId)
		{
			bool CheckRole(string roleName, int id) =>
				CommonServices.PermissionService.ValidateUserPresetPermission(roleName, id);

			var roles = new List<PayoutRequestUserRole>();

			if(CheckRole("role_financier_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Financier);
			}

			if(IsAuthorSubdivisionControlledByCurrentEmployee() || CurrentUser.IsAdmin)
			{
				roles.Add(PayoutRequestUserRole.SubdivisionChief);
			}

			if(CheckRole("role_coordinator_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Coordinator);
			}

			if(CheckRole(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier, userId))
			{
				roles.Add(PayoutRequestUserRole.Cashier);
			}

			if(Entity.Author == null || Entity.Author.Id == CurrentEmployee.Id)
			{
				roles.Add(PayoutRequestUserRole.RequestCreator);
			}

			if(CheckRole("role_security_service_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.SecurityService);
			}

			return roles;
		}

		private bool IsAuthorSubdivisionControlledByCurrentEmployee()
		{
			var subdivisionsControlledByCurrentEmployee =
				_employeeRepository.GetControlledByEmployeeSubdivisionIds(UoW, CurrentEmployee.Id);

			return subdivisionsControlledByCurrentEmployee.Contains(Entity.Author?.Subdivision?.Id ?? -1);
		}

		private void CreateNewExpenseForItem(CashRequestSumItem sumItem, decimal sum)
		{
			sumItem?.CreateNewExpense(
				UoW,
				CurrentEmployee,
				Entity.Subdivision,
				Entity.ExpenseCategoryId,
				Entity.Basis,
				Entity.Organization,
				sum);

			if(sumItem != null)
			{
				_sumsGiven.Add(sumItem);
			}
		}

		public string LoadOrganizationsSums()
		{
			var builder = new StringBuilder();

			var balanceList = _cashRepository.GetCashBalanceForOrganizations(UoW);

			foreach(var operationNode in balanceList)
			{
				builder.Append(operationNode.Name + ": ");
				builder.Append(operationNode.Balance + "\n");
			}

			return builder.ToString();
		}

		private void ChangeStateAndSave(PayoutRequestState newState)
		{
			var validationResult = Entity.RaiseValidationAndGetResult();

			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"{validationResult}");

				return;
			}

			Entity.ChangeState(newState);

			AfterSaveCommand.Execute();
		}

		private bool AfterSave(out string messageText)
		{
			if(_sumsGiven.Any())
			{
				var builder = new StringBuilder();
				builder.Append("Подотчетное лицо\tСумма\n");

				foreach(CashRequestSumItem sum in _sumsGiven)
				{
					builder.Append($"{sum.AccountableEmployee.Name}\t{sum.ObservableExpenses.Last().Money}\n");
				}

				messageText = builder.ToString();
				return true;
			}
			else
			{
				messageText = "";
				return false;
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnEntityPropertyChanged;
			base.Dispose();
		}
	}
}
