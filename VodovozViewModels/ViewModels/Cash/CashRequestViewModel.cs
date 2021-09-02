using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using Gamma.Utilities;
using NHibernate.Criterion;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class CashRequestViewModel: EntityTabViewModelBase<CashRequest>
    {
        public Action UpdateNodes;
        public Employee CurrentEmployee { get; }
        public IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory { get; }
        public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

        private readonly IEntityUoWBuilder uowBuilder;
        private readonly ICashRepository _cashRepository;
        public HashSet<CashRequestSumItem> SumsGiven = new HashSet<CashRequestSumItem>();
        
        public string StateName => Entity.PayoutRequestState.GetEnumTitle();
        public CashRequestViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IFileChooserProvider fileChooserProvider,
            IEmployeeRepository employeeRepository,
            ICashRepository cashRepository,
            IEmployeeJournalFactory employeeJournalFactory,
            ISubdivisionJournalFactory subdivisionJournalFactory
        ) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            this.uowBuilder = uowBuilder ?? throw new ArgumentNullException(nameof(uowBuilder));
            _cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
            EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
            var filterViewModel = new ExpenseCategoryJournalFilterViewModel {
                ExcludedIds = new CategoryRepository(new ParametersProvider()).ExpenseSelfDeliveryCategories(UoW).Select(x => x.Id),
                HidenByDefault = true
            };

            ExpenseCategoryAutocompleteSelectorFactory =
                new SimpleEntitySelectorFactory<ExpenseCategory, ExpenseCategoryViewModel>(
                    () =>
                    {
                        var expenseCategoryJournalViewModel =
                            new SimpleEntityJournalViewModel<ExpenseCategory, ExpenseCategoryViewModel>(
                                x => x.Name,
                                () => new ExpenseCategoryViewModel(
                                    EntityUoWBuilder.ForCreate(),
                                    unitOfWorkFactory,
                                    ServicesConfig.CommonServices,
                                    fileChooserProvider,
                                    filterViewModel,
                                    EmployeeJournalFactory,
                                    SubdivisionJournalFactory
                                ),
                                node => new ExpenseCategoryViewModel(
                                    EntityUoWBuilder.ForOpen(node.Id),
                                    unitOfWorkFactory,
                                    ServicesConfig.CommonServices,
                                    fileChooserProvider,
                                    filterViewModel,
                                    EmployeeJournalFactory,
                                    SubdivisionJournalFactory
                                ),
                                unitOfWorkFactory,
                                ServicesConfig.CommonServices
                            )
                            {
                                SelectionMode = JournalSelectionMode.Single
                            };
                        expenseCategoryJournalViewModel.SetFilter(filterViewModel,
                            filter => Restrictions.Not(Restrictions.In("Id", filter.ExcludedIds.ToArray())));

                        return expenseCategoryJournalViewModel;
                    });
                
                var expenseCategorySelectorFactory = 
            CurrentEmployee = employeeRepository.GetEmployeeForCurrentUser(UoW);

            if(uowBuilder.IsNewEntity)
                TabName = "Создание новой заявки на выдачу ДС";
            else
                TabName = $"{Entity.Title}";

            int userId = ServicesConfig.CommonServices.UserService.CurrentUserId;
            
            UserRoles = getUserRoles(userId);
            IsRoleChooserSensitive = UserRoles.Count() > 1;
            UserRole = UserRoles.First();

            IsNewEntity = uowBuilder.IsNewEntity;
            ConfigureEntityChangingRelations();
        }
        
        public IEmployeeJournalFactory EmployeeJournalFactory { get; }
        public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }

        protected void ConfigureEntityChangingRelations()
        {
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => StateName);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanEditOnlyCoordinator);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => SensitiveForFinancier);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => ExpenseCategorySensitive);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanEditSumSensitive);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => VisibleOnlyForStatusUpperThanCreated);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanGiveSum);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanAccept);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanApprove);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConveyForResults);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanReturnToRenegotiation);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanCancel);
            SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConfirmPossibilityNotToReconcilePayments);
        }

        #region Commands

        private DelegateCommand addSumCommand;
        public DelegateCommand AddSumCommand => addSumCommand ?? (addSumCommand = new DelegateCommand(
            () =>
            {
                var cashRequestItemViewModel = new CashRequestItemViewModel(
                    UoW,
                    CommonServices.InteractiveService,
                    NavigationManager,
                    UserRole,
                    EmployeeJournalFactory
                );

                cashRequestItemViewModel.Entity = new CashRequestSumItem() { AccountableEmployee = CurrentEmployee };

                cashRequestItemViewModel.EntityAccepted += (sender, args) =>
                {
                    if (args is CashRequestSumItemAcceptedEventArgs acceptedArgs)
                    {
                        Entity.AddItem(acceptedArgs.AcceptedEntity);
                        acceptedArgs.AcceptedEntity.CashRequest = Entity;
                    }
                };

                TabParent.AddSlaveTab(
                    this, cashRequestItemViewModel
                );
            }, () => true
        ));
        
        private DelegateCommand editSumCommand;
        public DelegateCommand EditSumCommand => editSumCommand ?? (editSumCommand = new DelegateCommand(
            () =>
            {
                var cashRequestItemViewModel = new CashRequestItemViewModel(
                    UoW,
                    CommonServices.InteractiveService,
                    NavigationManager,
                    UserRole,
                    EmployeeJournalFactory
                );

                cashRequestItemViewModel.Entity = SelectedItem;

                TabParent.AddSlaveTab(
                    this,
                    cashRequestItemViewModel);
            }, () => true
        ));
        
        private DelegateCommand deleteSumCommand;
        public DelegateCommand DeleteSumCommand => deleteSumCommand ?? (deleteSumCommand = new DelegateCommand(
            () => {
                if (Entity.ObservableSums.Contains(SelectedItem))
                {	
                    Entity.ObservableSums.Remove(SelectedItem);
                }
            }, () => true
        ));
        
        private DelegateCommand afterSaveCommand;
        public DelegateCommand AfterSaveCommand => afterSaveCommand ?? (afterSaveCommand = new DelegateCommand(
            () => {
                if (Entity.ExpenseCategory == null && UserRole == PayoutRequestUserRole.Cashier)
                {
                    CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Необходимо заполнить статью расхода");
                    return;
                }
                SaveAndClose();
                if (AfterSave(out var messageText))
                    CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,$"Cоздан следующие аванс:\n{messageText}" );
            }, () => true
        ));
            
        private DelegateCommand<CashRequestSumItem> giveSumCommand;
        public DelegateCommand<CashRequestSumItem> GiveSumCommand => 
            giveSumCommand ?? (giveSumCommand = new DelegateCommand<CashRequestSumItem>(
                (CashRequestSumItem sumItem) => GiveSum(sumItem),
                CanExecuteGive
        ));

        private DelegateCommand<(CashRequestSumItem, decimal)> giveSumPartiallyCommand;
        public DelegateCommand<(CashRequestSumItem, decimal)> GiveSumPartiallyCommand => 
            giveSumPartiallyCommand ?? (giveSumPartiallyCommand = new DelegateCommand<(CashRequestSumItem, decimal)>(
                ((CashRequestSumItem CashRequestSumItem, decimal Sum) parameters) => GiveSum(parameters.CashRequestSumItem, parameters.Sum),
                ((CashRequestSumItem CashRequestSumItem, decimal Sum) parameters) => CanExecuteGive(parameters.CashRequestSumItem)
        ));

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
            if (!Entity.Sums.Any())
            {
                return;
            }

            if (Entity.ExpenseCategory == null)
            {
                CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"У данной заявки не заполнена статья расхода");
                return;
            }

            var cashRequestSumItem = sumItem ?? Entity.ObservableSums.FirstOrDefault(x => !x.ObservableExpenses.Any());

            if (cashRequestSumItem == null)
            {
                return;
            }

            var alreadyGiven = cashRequestSumItem.ObservableExpenses.Sum(x => x.Money);

            var decimalSumToGive = sumToGive ?? cashRequestSumItem.Sum - alreadyGiven;

            if (decimalSumToGive <= 0)
            {
                return;
            }

            CreateNewExpenseForItem(cashRequestSumItem, decimalSumToGive);
            if(!Entity.PossibilityNotToReconcilePayments
                && alreadyGiven > 0
                && (alreadyGiven + decimalSumToGive) == cashRequestSumItem.Sum
                && Entity.ObservableSums.Count(x => x.Expenses.Sum(e => e.Money) != x.Sum) > 0)
            {
                Entity.ChangeState(PayoutRequestState.OnClarification);
            } else
            {
                Entity.ChangeState(PayoutRequestState.Closed);
            }
            AfterSaveCommand.Execute();
        }

        #endregion Commands

        #region Properties

        private PayoutRequestUserRole userRole;
        public PayoutRequestUserRole UserRole
        {
            get { 
                return userRole; 
            }
            set {
                SetField(ref userRole, value);
                OnPropertyChanged(() => CanEditOnlyCoordinator);
                OnPropertyChanged(() => SensitiveForFinancier);
                OnPropertyChanged(() => ExpenseCategorySensitive);
                OnPropertyChanged(() => CanEditSumVisible);
                OnPropertyChanged(() => VisibleOnlyForFinancer);
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

        CashRequestSumItem selectedItem;
        public CashRequestSumItem SelectedItem 
        { 
            get => selectedItem;
            set
            {
                SetField(ref selectedItem, value);
                OnPropertyChanged(() => CanEditSumSensitive);
            }
        }
        
        #region Editability

        public bool CanEditOnlyCoordinator => UserRole == PayoutRequestUserRole.Coordinator;

        
        public bool SensitiveForFinancier => (Entity.PayoutRequestState == PayoutRequestState.New ||
                                                              Entity.PayoutRequestState == PayoutRequestState.Agreed ||
                                                              Entity.PayoutRequestState == PayoutRequestState.GivenForTake) &&
                                                             UserRole == PayoutRequestUserRole.Financier;

        public bool ExpenseCategorySensitive => (Entity.PayoutRequestState == PayoutRequestState.New
                                             || Entity.PayoutRequestState == PayoutRequestState.Agreed
                                             || Entity.PayoutRequestState == PayoutRequestState.GivenForTake)
                                             && (UserRole == PayoutRequestUserRole.Financier || UserRole == PayoutRequestUserRole.Cashier);

        public bool CanEditSumVisible => UserRole == PayoutRequestUserRole.RequestCreator || UserRole == PayoutRequestUserRole.Coordinator;
        //редактировать можно только не выданные
        public bool CanEditSumSensitive => SelectedItem != null && !SelectedItem.ObservableExpenses.Any();


        #endregion Editability

        #region Visibility

        public bool VisibleOnlyForFinancer => UserRole == PayoutRequestUserRole.Financier;
        public bool VisibleOnlyForStatusUpperThanCreated => Entity.PayoutRequestState != PayoutRequestState.New;
        public bool ExpenseCategoryVisibility => UserRole == PayoutRequestUserRole.Cashier || UserRole == PayoutRequestUserRole.Financier;
        public bool CanConfirmPossibilityNotToReconcilePayments => Entity.ObservableSums.Count > 1 && Entity.PayoutRequestState == PayoutRequestState.Submited && UserRole == PayoutRequestUserRole.Coordinator;
        #endregion Visibility

        #region Permissions

        public bool CanEdit => PermissionResult.CanUpdate;
        public bool CanAddItems => CanEdit;
        public bool CanDeleteItems => CanEdit && SelectedItem != null;
        
        public bool CanGiveSum => UserRole == PayoutRequestUserRole.Cashier && (Entity.PayoutRequestState == PayoutRequestState.GivenForTake ||
                                                                              Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed);

        public bool CanDeleteSum => uowBuilder.IsNewEntity;
        //Подтвердить
        public bool CanAccept =>
            (Entity.PayoutRequestState == PayoutRequestState.New || Entity.PayoutRequestState == PayoutRequestState.OnClarification);

        //Согласовать
        public bool CanApprove => Entity.PayoutRequestState == PayoutRequestState.Submited && UserRole == PayoutRequestUserRole.Coordinator;
        public bool CanConveyForResults => UserRole == PayoutRequestUserRole.Financier && Entity.PayoutRequestState == PayoutRequestState.Agreed;
        public bool CanReturnToRenegotiation => Entity.PayoutRequestState == PayoutRequestState.Agreed ||
                                                Entity.PayoutRequestState == PayoutRequestState.GivenForTake ||
                                                Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed ||
                                                Entity.PayoutRequestState == PayoutRequestState.Canceled;

        public bool CanCancel => Entity.PayoutRequestState == PayoutRequestState.Submited ||
                                 Entity.PayoutRequestState == PayoutRequestState.OnClarification ||
                                 (Entity.PayoutRequestState == PayoutRequestState.Agreed && UserRole == PayoutRequestUserRole.Coordinator) ||
                                 (Entity.PayoutRequestState == PayoutRequestState.GivenForTake && UserRole == PayoutRequestUserRole.Coordinator);
        

        #endregion Permissions
        
        #endregion Properties

        #region Methods

        private IEnumerable<PayoutRequestUserRole> getUserRoles(int userId)
        {
            var roles = new List<PayoutRequestUserRole>();
            if (checkRole("role_financier_cash_request", userId))
                roles.Add(PayoutRequestUserRole.Financier);
            if (checkRole("role_coordinator_cash_request", userId))
                roles.Add(PayoutRequestUserRole.Coordinator);
            if (checkRole("role_сashier", userId))
                roles.Add(PayoutRequestUserRole.Cashier);
            if (Entity.Author == null || Entity.Author.Id == CurrentEmployee.Id)
                roles.Add(PayoutRequestUserRole.RequestCreator);

            if (roles.Count == 0)
                throw new Exception("Пользователь не подходит ни под одну из ролей, он не должен был иметь возможность сюда зайти");
            return roles;
        }

        public static bool checkRole(string roleName, int userId) => ServicesConfig.CommonServices.PermissionService
            .ValidateUserPresetPermission(roleName, userId);
        
        
        private void CreateNewExpenseForItem(CashRequestSumItem sumItem, decimal sum)
        {
            sumItem?.CreateNewExpense(
                UoW, 
                CurrentEmployee,
                Entity.Subdivision,
                Entity.ExpenseCategory,
                Entity.Basis,
                Entity.Organization,
                sum
            );
            if (sumItem != null)
                SumsGiven.Add(sumItem);
        }

        public string LoadOrganizationsSums()
        {
            var builder = new StringBuilder();

            var balanceList = _cashRepository.GetCashBalanceForOrganizations(UoW);
            foreach (var operationNode in balanceList)
            {
                builder.Append(operationNode.Name + ": ");
                builder.Append(operationNode.Balance + "\n");
            }
            return builder.ToString();
        }

        public bool AfterSave(out string messageText)
        {
            if (SumsGiven.Count != 0) {
                var builder = new StringBuilder();
                builder.Append("Подотчетное лицо\tСумма\n");
                foreach (CashRequestSumItem sum in SumsGiven)
                {
                    builder.Append(sum.AccountableEmployee.Name + "\t" + sum.ObservableExpenses.Last().Money + "\n");
                }
                messageText = builder.ToString();
                return true;
            } else {
                messageText = "";
                return false;
            }
        }

        //Подтвердить
        private DelegateCommand acceptCommand;
        public DelegateCommand AcceptCommand => acceptCommand ?? (acceptCommand = new DelegateCommand(
            () =>
            {
                Entity.ChangeState(PayoutRequestState.Submited);
                AfterSaveCommand.Execute();
            }, () => true
        ));
        //Согласовать
        private DelegateCommand approveCommand;
        public DelegateCommand ApproveCommand => approveCommand ?? (approveCommand = new DelegateCommand(
            () =>
            {
                Entity.ChangeState(PayoutRequestState.Agreed);
                AfterSaveCommand.Execute();
            }, () => true
        ));
        //Отменить
        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
            () =>
            {
                if (string.IsNullOrEmpty(Entity.CancelReason) && UserRole == PayoutRequestUserRole.Coordinator) {
                    CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,"Причина отмены должна быть заполнена");
                } else {
                    Entity.ChangeState(PayoutRequestState.Canceled);
                    AfterSaveCommand.Execute();
                }
            }, () => true
        ));
        //Передать на выдачу
        private DelegateCommand conveyForResultsCommand;
        public DelegateCommand ConveyForResultsCommand => conveyForResultsCommand ?? (conveyForResultsCommand = new DelegateCommand(
            () =>
            {
                if (Entity.PayoutRequestState == PayoutRequestState.Agreed && Entity.ExpenseCategory == null && UserRole == PayoutRequestUserRole.Cashier)
                {
                    CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
                        "Необходимо заполнить статью расхода");
                }
                else
                {
                    Entity.ChangeState(PayoutRequestState.GivenForTake);
                    AfterSaveCommand.Execute();
                }
               
            }, () => true
        ));
        
        //Отправить на пересогласование((вернуть)на уточнение)
        private DelegateCommand returnToRenegotiationCommand;
        public DelegateCommand ReturnToRenegotiationCommand => returnToRenegotiationCommand ?? (returnToRenegotiationCommand = new DelegateCommand(
            () =>
            {
                if (string.IsNullOrEmpty(Entity.ReasonForSendToReappropriate)){
                    CommonServices.InteractiveService.ShowMessage(
                        ImportanceLevel.Warning,
                        "Причина отправки на пересогласование должна быть заполнена"
                    );
                } else {
                    Entity.ChangeState(PayoutRequestState.OnClarification);
                    AfterSaveCommand.Execute();
                }
            }, () => true
        ));
        
        #endregion
    }
}
