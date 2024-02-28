using Autofac;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Settings.Cash;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Cash
{
	public partial class AdvanceReportViewModel : EntityTabViewModelBase<AdvanceReport>
	{
		private readonly ILogger<AdvanceReportViewModel> _logger;
		private readonly IAdvanceCashOrganisationDistributor _distributor;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IAccountableDebtsRepository _accountableDebtsRepository;
		private readonly IAdvanceReportSettings _advanceReportSettings;
		private readonly IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> _financialExpenseCategoryNodeInMemoryCacheRepository;
		private readonly ILifetimeScope _scope;
		private readonly IPermissionResult _entityPermissionResult;
		private decimal _debt = 0;
		private decimal _balance = 0;
		private decimal _closingSum = 0;

		private List<SelectableNode<Expense>> _advanceList = new List<SelectableNode<Expense>>();
		private FinancialExpenseCategory _financialExpenseCategory;

		public event Action<EventArgs> OnDebtsChanged;

		public AdvanceReportViewModel(
			ILogger<AdvanceReportViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IAdvanceCashOrganisationDistributor distributor,
			IEmployeeRepository employeeRepository,
			ICategoryRepository categoryRepository,
			IAccountableDebtsRepository accountableDebtsRepository,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IAdvanceReportSettings advanceReportSettings,
			ILifetimeScope scope,
			IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> domainEntityNodeInMemoryCacheRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(entityExtendedPermissionValidator is null)
			{
				throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_distributor = distributor
				?? throw new ArgumentNullException(nameof(distributor));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_categoryRepository = categoryRepository
				?? throw new ArgumentNullException(nameof(categoryRepository));
			_accountableDebtsRepository = accountableDebtsRepository
				?? throw new ArgumentNullException(nameof(accountableDebtsRepository));
			_advanceReportSettings = advanceReportSettings ?? throw new ArgumentNullException(nameof(advanceReportSettings));
			_scope = scope
				?? throw new ArgumentNullException(nameof(scope));
			_financialExpenseCategoryNodeInMemoryCacheRepository = domainEntityNodeInMemoryCacheRepository
				?? throw new ArgumentNullException(nameof(domainEntityNodeInMemoryCacheRepository));

			_entityPermissionResult = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(AdvanceReport));

			CanEditRectroactively = entityExtendedPermissionValidator.Validate(
				typeof(AdvanceReport),
				ServicesConfig.UserService.CurrentUserId,
				nameof(RetroactivelyClosePermission));

			CachedOrganizaions = UoW.GetAll<Organization>().ToList().AsReadOnly();

			if(IsNew)
			{

				if(!_entityPermissionResult.CanCreate)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"Отсутствуют права на создание приходного ордера");
					FailInitialize = true;
					return;
				}

				Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				Entity.Date = DateTime.Now;

				if(Entity.Casher == null)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
					FailInitialize = true;
					return;
				}

				Entity.Organisation = UoW.GetById<Organization>(_advanceReportSettings.DefaultAdvanceReportOrganizationId);
			}

			EmployeeViewModel = BuildEmployeeEntryViewModel();

			EmployeeViewModel.Changed += (_, _2) => UpdateDebts(); // TODO: исправить 2й аргумент при переходе на C# 9

			CashierViewModel = BuildCashierEntryViewModel();

			UpdateDebts();

			FinancialExpenceCategoryViewModel = BuildFinancialExpenseCategoryEntryViewModel();

			FinancialExpenceCategoryViewModel.Changed += (_, _2) => UpdateDebts();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			SelectableAdvances.ForEach(x => x.SelectChanged += OnAdvanceSelectionChanged);

			SetPropertyChangeRelation(
				e => e.Id,
				() => IsNew);

			SetPropertyChangeRelation(
				e => e.Date,
				() => CanEdit);

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			SetPropertyChangeRelation(
				e => e.Money,
				() => Money);

			SetPropertyChangeRelation(
				e => e.RouteList,
				() => HasRouteList,
				() => RouteListTitle);

			SaveCommand = new DelegateCommand(SaveAndClose, () => CanEdit);
			CloseCommand = new DelegateCommand(() => Close(true, CloseSource.Self));

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Organisation))
			{
				UpdateDebts();
			}
		}

		#region Commands

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		#endregion Commands

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region EntityEntry ViewModels

		public IEntityEntryViewModel FinancialExpenceCategoryViewModel { get; }

		public IEntityEntryViewModel BuildFinancialExpenseCategoryEntryViewModel()
		{
			var expenseCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<AdvanceReportViewModel>(this, this, UoW, NavigationManager, _scope);

			return expenseCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
					})
				.Finish();
		}

		public IEntityEntryViewModel EmployeeViewModel { get; }

		public IEntityEntryViewModel BuildEmployeeEntryViewModel()
		{
			var employeeEntryViewModelBuilder = new CommonEEVMBuilderFactory<AdvanceReport>(this, Entity, UoW, NavigationManager, _scope);

			return employeeEntryViewModelBuilder
				.ForProperty(x => x.Accountable)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					})
				.Finish();
		}

		public IEntityEntryViewModel CashierViewModel { get; }

		public IEntityEntryViewModel BuildCashierEntryViewModel()
		{
			var cashierEntryViewModelBuilder = new CommonEEVMBuilderFactory<AdvanceReport>(this, Entity, UoW, NavigationManager, _scope);

			var viewModel = cashierEntryViewModelBuilder
				.ForProperty(x => x.Casher)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					})
				.Finish();

			viewModel.IsEditable = false;

			return viewModel;
		}

		#endregion EntityEntry ViewModels

		public IReadOnlyCollection<Organization> CachedOrganizaions { get; }

		[PropertyChangedAlso(nameof(DebtString))]
		public decimal Debt
		{
			get => _debt;
			set => SetField(ref _debt, value);
		}

		public string DebtString => $"{Debt:C}";

		[PropertyChangedAlso(nameof(ClosingSumEqualsMoney))]
		public decimal Money
		{
			get => Entity.Money;
			set
			{
				if(Entity.Money != value)
				{
					Entity.Money = value;
					Entity.NeedValidateOrganisation = ClosingSumNotEqualsMoney;
					CalculateBalance();
				}
			}
		}

		[PropertyChangedAlso(
			nameof(ClosingSumEqualsMoney),
			nameof(CreatingMessage),
			nameof(ChangeSumMessage),
			nameof(ChangeTypeMessage),
			nameof(CreatingMessageState),
			nameof(ClosingSumString))]
		public decimal ClosingSum
		{
			get => _closingSum;
			set
			{
				if(SetField(ref _closingSum, value))
				{
					CalculateBalance();
				}
			}
		}

		[PropertyChangedAlso(
			nameof(CreatingMessage),
			nameof(ChangeSumMessage),
			nameof(ChangeTypeMessage),
			nameof(CreatingMessageState),
			nameof(ChangeSumWarning))]
		public decimal Balance
		{
			get => _balance;
			set => SetField(ref _balance, value);
		}

		public string CreatingMessage =>
			ClosingSum == 0
			? "Нет выбранных авансов."
			: Balance == 0
				? "Аванс будет закрыт полностью."
				: Balance < 0
					? $"Будет создан расходный ордер на сумму {Math.Abs(Balance):C}, в качестве доплаты."
					: $"Будет создан приходный ордер на сумму {Math.Abs(Balance):C}, в качестве сдачи от подотчетного лица.";


		public CreatingMessageState CreatingMessageState =>
			ClosingSum == 0
			? CreatingMessageState.ClosingSumZero
			: Balance == 0
				? CreatingMessageState.BalanceZero 
				: Balance < 0
					? CreatingMessageState.BalanceLessThanZero
					: CreatingMessageState.BalanceGreaterThanZero;

		public string ChangeTypeMessage =>
			ClosingSum == 0 || ClosingSumEqualsMoney
				? string.Empty
				: Balance == 0
					? "Доплата:"
					: "Остаток:";

		public string ChangeSumMessage =>
			ClosingSum == 0
				? string.Empty
				: Balance == 0
					? string.Empty
					: Balance < 0
						? $"{Math.Abs(Balance):C}"
						: $"{Balance:C}";

		public bool ChangeSumWarning => ClosingSum != 0 && Balance < 0;

		public string CurrencySymbol => NumberFormatInfo.CurrentInfo.CurrencySymbol;

		public bool ClosingSumNotEqualsMoney => !ClosingSumEqualsMoney;

		[PropertyChangedAlso(nameof(ClosingSumNotEqualsMoney))]
		public bool ClosingSumEqualsMoney => ClosingSum == Money;

		public bool CanCreate => _entityPermissionResult.CanCreate;

		public bool CanEdit => (UoW.IsNew && CanCreate)
			|| (_entityPermissionResult.CanUpdate
				&& Entity.Date.Date == DateTime.Now.Date)
			|| CanEditRectroactively;

		public bool IsNew => UoWGeneric.IsNew;

		public bool HasRouteList => Entity.RouteList != null;

		public string RouteListTitle => Entity.RouteList?.Title ?? string.Empty;

		public List<SelectableNode<Expense>> SelectableAdvances => _advanceList;

		public bool CanEditRectroactively { get; }

		public string ClosingSumString => $"{ClosingSum:C}";

		public override bool Save(bool close)
		{
			_logger.LogInformation("Сохраняем авансовый отчет...");

			bool needClosing = UoWGeneric.IsNew;

			if(!base.Save(false))
			{
				return false;
			}

			if(needClosing)
			{
				var closing = Entity.CloseAdvances(
					out Expense newExpense,
					out Income newIncome,
					SelectableAdvances
						.Where(a => a.Selected)
						.Select(a => a.Value)
						.ToList());

				if(newExpense != null)
				{
					UoWGeneric.Save(newExpense);
					_logger.LogInformation("Создаем документ распределения расхода налички по юр лицу...");
					_distributor.DistributeCashForExpenseAdvance(UoW, newExpense, Entity);
				}

				if(newIncome != null)
				{
					UoWGeneric.Save(newIncome);
					_logger.LogInformation("Создаем документ распределения прихода налички по юр лицу...");
					_distributor.DistributeCashForIncomeAdvance(UoW, newIncome, Entity);
				}

				SelectableAdvances
					.Where(a => a.Selected)
					.Select(a => a.Value)
					.ToList()
					.ForEach(a => UoWGeneric.Save(a));

				closing.ForEach(c => UoWGeneric.Save(c));

				if(Entity.RouteList != null)
				{
					_logger.LogInformation("Обновляем сумму долга по МЛ...");
					Entity.RouteList.UpdateRouteListDebt();
					_logger.LogInformation("Ok");
				}

				UoWGeneric.Save();

				OnPropertyChanged(nameof(IsNew));

				if(newIncome != null)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						$"Дополнительно создан приходный ордер №{newIncome.Id}," +
						$" на сумму {newIncome.Money:C}.\nНе забудьте получить сдачу от подотчетного лица!");
				}
				if(newExpense != null)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						$"Дополнительно создан расходный ордер №{newExpense.Id}," +
						$" на сумму {newExpense.Money:C}.\nНе забудьте доплатить подотчетному лицу!");
				}
			}

			_logger.LogInformation("Ok");

			if(close)
			{
				Close(false, CloseSource.Save);
			}

			return true;
		}

		private void ClearDebts()
		{
			Money = 0m;
			ClosingSum = 0m;

			if(SelectableAdvances.Any())
			{
				SelectableAdvances
					.ForEach(advance => advance.SelectChanged -= OnAdvanceSelectionChanged);

				SelectableAdvances.Clear();

				OnDebtsChanged?.Invoke(EventArgs.Empty);
			}
		}

		private void UpdateDebts()
		{
			if(!UoW.IsNew)
			{
				return;
			}

			if(Entity.Accountable == null)
			{
				Debt = 0;
				ClearDebts();
				return;
			}

			_logger.LogInformation(
				"Получаем долг {Employee}...",
				Entity.Accountable.ShortName);

			var advances =
				_accountableDebtsRepository.GetUnclosedAdvances(
					UoW,
					Entity.Accountable,
					Entity.ExpenseCategoryId,
					Entity.Organisation?.Id);

			Debt = advances.Sum(a => a.UnclosedMoney);

			ClearDebts();

			_financialExpenseCategoryNodeInMemoryCacheRepository
				.WarmUpCacheWithIds(advances
					.Where(x => x.ExpenseCategoryId != null)
					.Select(x => x.ExpenseCategoryId.Value));

			SelectableAdvances.AddRange(
				advances.Select(advance => SelectableNode<Expense>.Create(advance)));

			SelectableAdvances
				.ForEach(advance => advance.SelectChanged += OnAdvanceSelectionChanged);

			CalculateBalance();

			OnDebtsChanged?.Invoke(EventArgs.Empty);

			_logger.LogInformation("Ok");
		}

		public void CalculateBalance()
		{
			if(!UoW.IsNew)
			{
				return;
			}

			Balance = ClosingSum - Money;
		}

		public void InitializationFailed(
			string title = "",
			string message = "")
		{
			if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(message))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, message, title);
			}

			FailInitialize = true;
		}

		private void OnAdvanceSelectionChanged(object sender, SelectionChanged<Expense> e)
		{
			if(e.SelectableNode.Value.RouteListClosing != null
				&& SelectableAdvances.Any(x => x.Value.RouteListClosing != null && x.Selected))
			{
				SelectableAdvances
					.Where(expense => expense.Value.RouteListClosing?.Id != e.SelectableNode.Value.RouteListClosing.Id)
					.ToList()
					.ForEach(selectedExpense => selectedExpense.SilentUnselect());
			}

			UpdateRouteList();

			ClosingSum = SelectableAdvances
				.Where(a => a.Selected)
				.Sum(a => a.Value.UnclosedMoney);

			Money = ClosingSum;
		}

		private void UpdateRouteList()
		{
			if(!UoW.IsNew)
			{
				return;
			}

			var selectedAdvances = SelectableAdvances
				.Where(a => a.Selected)
				.Select(a => a.Value.RouteListClosing)
				.ToList();

			var selectedRouteListsCount = selectedAdvances?.GroupBy(rl => rl?.Id).Count();

			if(selectedRouteListsCount != 1)
			{
				Entity.RouteList = null;
				return;
			}

			Entity.RouteList = selectedAdvances.FirstOrDefault();
		}

		public void ConfigureForReturn(int expenseId)
		{
			var expense = UoW.GetById<Expense>(expenseId);

			if(expense.Employee is null)
			{
				InitializationFailed("Ошибка", "Аванс без сотрудника. Для него нельзя открыть диалог возврата.");
				return;
			}

			Entity.Accountable = expense.Employee;
			Entity.ExpenseCategoryId = expense.ExpenseCategoryId;
			Entity.Organisation = expense.Organisation;
			Money = expense.UnclosedMoney;

			var advanceToSelect = SelectableAdvances
				.Where(x => x.Value.Id == expenseId)
				.FirstOrDefault();

			if(advanceToSelect is null)
			{
				return;
			}

			advanceToSelect.Selected = true;
		}

		public string GetCachedExpenseCategoryTitle(int id) =>
			_financialExpenseCategoryNodeInMemoryCacheRepository.GetTitleById(id);
	}

	public enum CreatingMessageState
	{
		ClosingSumZero,
		BalanceZero,
		BalanceLessThanZero,
		BalanceGreaterThanZero
	}
}
