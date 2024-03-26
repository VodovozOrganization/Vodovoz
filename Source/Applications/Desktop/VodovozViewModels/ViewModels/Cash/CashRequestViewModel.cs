﻿using Autofac;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.NotificationRecievers;
using Vodovoz.Tools;
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
		private readonly ICashRepository _cashRepository;
		private readonly HashSet<CashRequestSumItem> _sumsGiven = new HashSet<CashRequestSumItem>();
		private readonly ILifetimeScope _scope;
		private FinancialExpenseCategory _financialExpenseCategory;
		private readonly DriverAPIHelper _driverAPIHelper;
		private bool _needToNotifyDriverOfReadyToGiveOut = false;

		public CashRequestViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			ICashRepository cashRepository,
			INavigationManager navigation,
			ILifetimeScope scope,
			DriverAPIHelper driverAPIHelper)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(employeeRepository is null)
			{
				throw new ArgumentNullException(nameof(employeeRepository));
			}

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));

			IsNewEntity = uowBuilder?.IsNewEntity ?? throw new ArgumentNullException(nameof(uowBuilder));

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_driverAPIHelper = driverAPIHelper ?? throw new ArgumentNullException(nameof(driverAPIHelper));
			CurrentEmployee = employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(UoWGeneric.IsNew)
			{
				Entity.Author = CurrentEmployee;
			}

			Entity.Subdivision = CurrentEmployee.Subdivision;

			TabName = IsNewEntity ? "Создание новой заявки на выдачу ДС" : $"{Entity.Title}";

			UserRoles = GetUserRoles(CurrentUser.Id);
			IsRoleChooserSensitive = UserRoles.Count() > 1;
			UserRole = UserRoles.First();

			ConfigureEntityChangingRelations();

			var authorEntryViewModelBuilder = new CommonEEVMBuilderFactory<CashRequest>(this, Entity, UoW, NavigationManager, _scope);

			AuthorViewModel = authorEntryViewModelBuilder
				.ForProperty(x => x.Author)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			var subdivisionEntryViewModelBuilder = new CommonEEVMBuilderFactory<CashRequest>(this, Entity, UoW, NavigationManager, _scope);

			SubdivisionViewModel = subdivisionEntryViewModelBuilder
				.ForProperty(x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			var expenseCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<CashRequestViewModel>(this, this, UoW, NavigationManager, _scope);

			FinancialExpenseCategoryViewModel = expenseCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			ApproveCommand = new DelegateCommand(() =>
				{
					ChangeStateAndSave(PayoutRequestState.Agreed);
				},
				() => true);

			AcceptCommand = new DelegateCommand(() =>
				{
					ChangeStateAndSave(PayoutRequestState.Submited);
				},
				() => true);

			CancelCommand = new DelegateCommand(() =>
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
				},
				() => true);

			ConveyForResultsCommand = new DelegateCommand(() =>
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
						_needToNotifyDriverOfReadyToGiveOut = true;
						ChangeStateAndSave(PayoutRequestState.GivenForTake);
						_needToNotifyDriverOfReadyToGiveOut = false;
					}
				},
				() => true);

			ReturnToRenegotiationCommand = new DelegateCommand(() =>
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
					},
					() => true);

			AddSumCommand = new DelegateCommand(
				() =>
				{
					var cashRequestItemPage = NavigationManager
						.OpenViewModel<CashRequestItemViewModel, IUnitOfWork, PayoutRequestUserRole>(this, UoW, UserRole, OpenPageOptions.AsSlave);

					cashRequestItemPage.ViewModel.Entity = new CashRequestSumItem()
					{
						AccountableEmployee = CurrentEmployee
					};

					cashRequestItemPage.ViewModel.EntityAccepted += (sender, args) =>
					{
						if(args is CashRequestSumItemAcceptedEventArgs acceptedArgs)
						{
							Entity.AddItem(acceptedArgs.AcceptedEntity);
							acceptedArgs.AcceptedEntity.CashRequest = Entity;
						}
					};
				}, () => true
			);

			EditSumCommand = new DelegateCommand(
				() =>
				{
					var cashRequestItemPage = NavigationManager
						.OpenViewModel<CashRequestItemViewModel, IUnitOfWork, PayoutRequestUserRole>(this, UoW, UserRole, OpenPageOptions.AsSlave);

					cashRequestItemPage.ViewModel.Entity = SelectedItem;
				}, () => true
			);

			DeleteSumCommand = new DelegateCommand(
				() =>
				{
					if(Entity.ObservableSums.Contains(SelectedItem))
					{
						Entity.ObservableSums.Remove(SelectedItem);
					}
				}, () => true
			);

			AfterSaveCommand = new DelegateCommand(
				() =>
				{
					if(Entity.ExpenseCategoryId == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
							"Необходимо заполнить статью расхода");
						return;
					}

					var entityId = Entity.Id;

					SaveAndClose();

					_driverAPIHelper.NotifyOfCashRequestForDriverIsGivenForTake(entityId);

					if(AfterSave(out var messageText))
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
							$"Cоздан следующие аванс:\n{messageText}");
					}
				}, () => true
			);

			GiveSumCommand = new DelegateCommand<CashRequestSumItem>(
				(sumItem) => GiveSum(sumItem),
				CanExecuteGive
			);

			GiveSumPartiallyCommand = new DelegateCommand<(CashRequestSumItem, decimal)>(
				((CashRequestSumItem CashRequestSumItem, decimal Sum) parameters) => GiveSum(parameters.CashRequestSumItem, parameters.Sum),
				((CashRequestSumItem CashRequestSumItem, decimal Sum) parameters) => CanExecuteGive(parameters.CashRequestSumItem)
			);
		}

		#region Статья расхода
		private bool _hasFinancialExpenseCategoryPermission => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.FinancialCategory.CanChangeFinancialExpenseCategory);
		private PayoutRequestState[] _expenseCategoriesForAll => new[] { PayoutRequestState.New, PayoutRequestState.OnClarification, PayoutRequestState.Submited };
		private PayoutRequestState[] _expenseCategoriesWithSpecialPermission => new[] { PayoutRequestState.Agreed, PayoutRequestState.GivenForTake, PayoutRequestState.PartiallyClosed };
		#endregion

		public Employee CurrentEmployee { get; }

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; }

		public IEntityEntryViewModel AuthorViewModel { get; }

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => StateName);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanEditOnlyCoordinator);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => SensitiveForFinancier);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => ExpenseCategorySensitive);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanEditSumSensitive);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => VisibleOnlyForStatusUpperThanCreated);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSeeGiveSum);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanAccept);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanApprove);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConveyForResults);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanReturnToRenegotiation);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanCancel);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConfirmPossibilityNotToReconcilePayments);
			SetPropertyChangeRelation(e => e.ObservableSums, () => CanGiveSum);
		}

		#region Commands

		public DelegateCommand AddSumCommand { get; }

		public DelegateCommand EditSumCommand { get; }

		public DelegateCommand DeleteSumCommand { get; }

		public DelegateCommand AfterSaveCommand { get; }

		public DelegateCommand<CashRequestSumItem> GiveSumCommand { get; }

		public DelegateCommand<(CashRequestSumItem, decimal)> GiveSumPartiallyCommand { get; }

		public string StateName => Entity.PayoutRequestState.GetEnumTitle();

		public bool CanExecuteGive(CashRequestSumItem sumItem)
		{
			return sumItem != null
				&& sumItem.Sum > sumItem.Expenses.Sum(e => e.Money)
				&& (Entity.PossibilityNotToReconcilePayments
				 || sumItem.Expenses.Any()
				 || Entity.ObservableSums.All(x => !x.Expenses.Any() || x.Sum == x.Expenses.Sum(e => e.Money))
				   );
		}

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

			var cashRequestSumItem = sumItem ?? Entity.ObservableSums.FirstOrDefault(x => !x.ObservableExpenses.Any());

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

		#endregion Commands

		#region Properties

		private PayoutRequestUserRole _userRole;

		public PayoutRequestUserRole UserRole
		{
			get => _userRole;
			set
			{
				SetField(ref _userRole, value);
				OnPropertyChanged(() => CanEditOnlyCoordinator);
				OnPropertyChanged(() => SensitiveForFinancier);
				OnPropertyChanged(() => ExpenseCategorySensitive);
				OnPropertyChanged(() => VisibleOnlyForFinancer);
				OnPropertyChanged(() => CanSeeGiveSum);
				OnPropertyChanged(() => CanGiveSum);
				OnPropertyChanged(() => CanApprove);
				OnPropertyChanged(() => CanConveyForResults);
				OnPropertyChanged(() => CanCancel);
				OnPropertyChanged(() => CanConfirmPossibilityNotToReconcilePayments);
				OnPropertyChanged(() => ExpenseCategoryVisibility);
			}
		}


		public bool IsNewEntity { get; private set; }
		public bool IsRoleChooserSensitive { get; set; }

		private CashRequestSumItem _selectedItem;

		public CashRequestSumItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				SetField(ref _selectedItem, value);
				OnPropertyChanged(() => CanEditSumSensitive);
			}
		}

		#region Editability

		public bool CanEditOnlyCoordinator => UserRole == PayoutRequestUserRole.Coordinator;

		public bool SensitiveForFinancier => UserRole == PayoutRequestUserRole.Financier
										  && (Entity.PayoutRequestState == PayoutRequestState.New
										   || Entity.PayoutRequestState == PayoutRequestState.Agreed
										   || Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		public bool ExpenseCategorySensitive => _expenseCategoriesForAll.Contains(Entity.PayoutRequestState)
											  || (_expenseCategoriesWithSpecialPermission.Contains(Entity.PayoutRequestState) && _hasFinancialExpenseCategoryPermission);

		//редактировать можно только не выданные
		public bool CanEditSumSensitive => SelectedItem != null
										&& !SelectedItem.ObservableExpenses.Any();

		#endregion Editability

		#region Visibility

		public bool VisibleOnlyForFinancer => UserRole == PayoutRequestUserRole.Financier;
		public bool VisibleOnlyForStatusUpperThanCreated => Entity.PayoutRequestState != PayoutRequestState.New;

		public bool ExpenseCategoryVisibility => true;

		public bool CanConfirmPossibilityNotToReconcilePayments => Entity.ObservableSums.Count > 1
																&& Entity.PayoutRequestState == PayoutRequestState.Submited
																&& UserRole == PayoutRequestUserRole.Coordinator;

		#endregion Visibility

		#region Permissions

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanSeeGiveSum => UserRole == PayoutRequestUserRole.Cashier
								  && (Entity.PayoutRequestState == PayoutRequestState.GivenForTake
								   || Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed);

		public bool CanGiveSum => UserRole == PayoutRequestUserRole.Cashier
								  && (Entity.PayoutRequestState == PayoutRequestState.GivenForTake
								   || Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed);

		public bool CanDeleteSum => IsNewEntity;

		//Подтвердить
		public bool CanAccept => Entity.PayoutRequestState == PayoutRequestState.New
							  || Entity.PayoutRequestState == PayoutRequestState.OnClarification;

		//Согласовать
		public bool CanApprove => Entity.PayoutRequestState == PayoutRequestState.Submited
							   && UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanConveyForResults => UserRole == PayoutRequestUserRole.Financier
										&& Entity.PayoutRequestState == PayoutRequestState.Agreed;

		public bool CanReturnToRenegotiation => Entity.PayoutRequestState == PayoutRequestState.Agreed
											 || Entity.PayoutRequestState == PayoutRequestState.GivenForTake
											 || Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed
											 || Entity.PayoutRequestState == PayoutRequestState.Canceled;

		public bool CanCancel => Entity.PayoutRequestState == PayoutRequestState.Submited
							  || Entity.PayoutRequestState == PayoutRequestState.OnClarification
							  || Entity.PayoutRequestState == PayoutRequestState.Agreed
							  && UserRole == PayoutRequestUserRole.Coordinator
							  || Entity.PayoutRequestState == PayoutRequestState.GivenForTake
							  && UserRole == PayoutRequestUserRole.Coordinator;

		#endregion Permissions

		public bool IsSecurityServiceRole => UserRole == PayoutRequestUserRole.SecurityService;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => !IsSecurityServiceRole;

		#endregion

		#endregion Properties

		private IEnumerable<PayoutRequestUserRole> GetUserRoles(int userId)
		{
			bool CheckRole(string roleName, int id) =>
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(roleName, id);

			var roles = new List<PayoutRequestUserRole>();
			if(CheckRole("role_financier_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Financier);
			}

			if(CheckRole("role_coordinator_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Coordinator);
			}

			if(CheckRole(Vodovoz.Permissions.Cash.RoleCashier, userId))
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

			if(roles.Count == 0)
			{
				throw new Exception("Пользователь не подходит ни под одну из ролей, он не должен был иметь возможность сюда зайти");
			}

			return roles;
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
				sum
			);
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
					builder.Append(sum.AccountableEmployee.Name + "\t" + sum.ObservableExpenses.Last().Money + "\n");
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

		public DelegateCommand AcceptCommand { get; }

		public DelegateCommand ApproveCommand { get; }

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand ConveyForResultsCommand { get; }

		public DelegateCommand ReturnToRenegotiationCommand { get; }
	}
}
