using Autofac;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.PermissionExtensions;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using DateTimeHelpers;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using QS.Report;

namespace Vodovoz.ViewModels.Cash
{
	public class ExpenseViewModel : EntityTabViewModelBase<Expense>
	{
		private readonly ILogger<ExpenseViewModel> _logger;

		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWagesMovementRepository _wagesMovementRepository;
		private readonly IAccountableDebtsRepository _accountableDebtsRepository;
		private readonly IExpenseSettings _expenseSettings;
		private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
		private readonly IRouteListCashOrganisationDistributor _routeListCashOrganisationDistributor;
		private readonly IExpenseCashOrganisationDistributor _expenseCashOrganisationDistributor;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IFuelCashOrganisationDistributor _fuelCashOrganisationDistributor;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IUserService _userService;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IPermissionResult _entityPermissionResult;
		private readonly IReportViewOpener _reportViewOpener;
		private FinancialExpenseCategory _financialExpenseCategory;
		private bool _canEditDate;
		private bool _canEditDdrDate;
		private Employee _restrictEmployee;

		public ExpenseViewModel(
			ILogger<ExpenseViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			IWagesMovementRepository wagesMovementRepository,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IRouteListCashOrganisationDistributor routeListCashOrganisationDistributor,
			IExpenseCashOrganisationDistributor expenseCashOrganisationDistributor,
			IFuelCashOrganisationDistributor fuelCashOrganisationDistributor,
			IUserService userService,
			ICategoryRepository categoryRepository,
			ILifetimeScope lifetimeScope,
			IReportViewOpener reportViewOpener,
			IAccountableDebtsRepository accountableDebtsRepository,
			IExpenseSettings expenseSettings,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IReportInfoFactory reportInfoFactory
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_wagesMovementRepository = wagesMovementRepository
				?? throw new ArgumentNullException(nameof(wagesMovementRepository));
			_entityExtendedPermissionValidator = entityExtendedPermissionValidator
				?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			_routeListCashOrganisationDistributor = routeListCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(routeListCashOrganisationDistributor));
			_expenseCashOrganisationDistributor = expenseCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(expenseCashOrganisationDistributor));
			_fuelCashOrganisationDistributor = fuelCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(fuelCashOrganisationDistributor));
			_userService = userService
				?? throw new ArgumentNullException(nameof(userService));
			_categoryRepository = categoryRepository
				?? throw new ArgumentNullException(nameof(categoryRepository));
			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));
			_reportViewOpener = reportViewOpener
				?? throw new ArgumentNullException(nameof(reportViewOpener));
			_accountableDebtsRepository = accountableDebtsRepository
				?? throw new ArgumentNullException(nameof(accountableDebtsRepository));
			_expenseSettings = expenseSettings ?? throw new ArgumentNullException(nameof(expenseSettings));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings
				?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_entityPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Expense));

			CanEditRectroactively =
				_entityExtendedPermissionValidator.Validate(
					typeof(Expense), userService.CurrentUserId, nameof(RetroactivelyClosePermission));

			_canEditDate = commonServices.CurrentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.Expense.CanEditDate);

			_canEditDdrDate = commonServices.CurrentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.Expense.CanEditDdrDate);

			CachedOrganizations = UoW.GetAll<Organization>().ToList().AsReadOnly();

			if(IsNew)
			{
				Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(Entity.Casher == null)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
						"Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
					FailInitialize = true;
					return;
				}

				if(!CanCreate)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Отсутствуют права на создание расходного ордера");
					FailInitialize = true;
					return;
				}

				Entity.Organisation = CachedOrganizations
					.Where(x => x.Id == _expenseSettings.DefaultExpenseOrganizationId)
					.FirstOrDefault();

				Entity.Date = DateTime.Now;
			}

			EmployeeViewModel = BuildEmployeeViewModel();

			CashierViewModel = BuildCashierEntryViewModel();

			RouteListViewModel = BuildRouteListEntitiViewModel();

			FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.Id,
				() => IsNew);

			SetPropertyChangeRelation(
				e => e.Date,
				() => CanEdit);

			SetPropertyChangeRelation(
				e => e.DdrDate,
				() => DdrDate);

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			SetPropertyChangeRelation(
				e => e.Employee,
				() => EmployeeTypeString,
				() => EmployeeBalanceVisible,
				() => CurrentEmployeeWageBalanceLabelString);

			SetPropertyChangeRelation(
				e => e.TypeOperation,
				() => EmployeeTypeString,
				() => EmployeeBalanceVisible,
				() => CanEditDate,
				() => OrganisationVisible,
				() => IsAdvance);

			SetPropertyChangeRelation(
				e => e.Money,
				() => Money);

			Entity.PropertyChanged += OnEntityPropertyChanged;

			PrintCommand = new DelegateCommand(Print);
			SaveCommand = new DelegateCommand(SaveAndClose, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, vm => vm.CanSave);

			CloseCommand = new DelegateCommand(() => Close(CanSave, CloseSource.Cancel));

			RefreshCurrentEmployeeWage();

			ValidationContext.Items.Add(
				nameof(IFinancialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId),
				_financialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId);

			PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(CanEdit))
			{
				OnPropertyChanged(nameof(CanSave));
			}
		}

		public IReadOnlyCollection<Organization> CachedOrganizations { get; }

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region Commands

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		public DelegateCommand PrintCommand { get; }

		#endregion Commands

		#region EntityEntry ViewModels

		public IEntityEntryViewModel EmployeeViewModel { get; }

		private IEntityEntryViewModel BuildEmployeeViewModel()
		{
			var employeeEntryViewModelBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			return employeeEntryViewModelBuilder
				.ForProperty(x => x.Employee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					})
				.Finish();
		}

		public IEntityEntryViewModel CashierViewModel { get; }

		private IEntityEntryViewModel BuildCashierEntryViewModel()
		{
			var cashierEntryViewModelBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, NavigationManager, _lifetimeScope);

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

		public IEntityEntryViewModel RouteListViewModel { get; }

		private IEntityEntryViewModel BuildRouteListEntitiViewModel()
		{
			var routeListEntryViewModelBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = routeListEntryViewModelBuilder
				.ForProperty(x => x.RouteListClosing)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(
					filter =>
					{
						filter.DisplayableStatuses = new[]
						{
							RouteListStatus.New,
							RouteListStatus.Confirmed,
							RouteListStatus.InLoading,
							RouteListStatus.EnRoute,
							RouteListStatus.Delivered,
							RouteListStatus.OnClosing,
							RouteListStatus.MileageCheck,
							RouteListStatus.Closed
						};

						filter.StartDate = DateTime.Today.AddDays(-3);
						filter.EndDate = DateTime.Today.AddDays(1);
					})
				.Finish();

			return viewModel;
		}

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialExpenseCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<ExpenseViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			return financialExpenseCategoryViewModelEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.TargetDocument = Entity.TypeDocument.ToTargetDocument();
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();
		}

		#endregion EntityEntry ViewModels

		public string CurrencySymbol => NumberFormatInfo.CurrentInfo.CurrencySymbol;

		public decimal Money
		{
			get => Entity.Money;
			set => Entity.Money = value;
		}

		public DateTime DdrDate
		{
			get => Entity.DdrDate;
			set
			{
				if(DdrDate != default && DdrDate != Entity.DdrDate && !CanEditDdrDate)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"У вас недостаточно прав для изменения даты учета ДДР");
					return;
				}

				var dateTimeLowerBorder = DateTimeExtensions.Max(Entity.Date, Entity.DdrDate.FirstDayOfMonth());

				if(value.Date >= dateTimeLowerBorder.Date)
				{
					Entity.DdrDate = value.Date;
				}
				else
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"Нельзя установить дату учета ДДР ранее {dateTimeLowerBorder:dd.MM.yyyy}");
					OnPropertyChanged(nameof(DdrDate));
				}
			}
		}

		public bool CanEditRectroactively { get; }

		public bool CanEditDate => _canEditDate && !IsAdvance;

		public bool CanEditDdrDate => _canEditDdrDate;

		public bool CanCreate => _entityPermissionResult.CanCreate;

		public bool CanEdit => (UoW.IsNew && CanCreate)
			|| (_entityPermissionResult.CanUpdate && Entity.Date.Date == DateTime.Now.Date)
			|| CanEditRectroactively;

		public bool CanSave => CanEdit || CanEditDate;

		[PropertyChangedAlso(nameof(CurrentEmployeeWageBalanceLabelString))]
		public decimal CurrentEmployeeWage { get; private set; }

		public string CurrentEmployeeWageString => $"<span font='large' weight='bold'>Текущий баланс сотрудника: {CurrentEmployeeWage:C}</span>";

		[PropertyChangedAlso(nameof(CurrentEmployeeWageBalanceLabelString))]
		public decimal CurrentDriverFutureFinesBalance { get; private set; }

		public string FutureEmployeeWageString => $"      <span font='large' weight='bold'>Будущие штрафы: {-CurrentDriverFutureFinesBalance:C}</span>";

		public string CurrentEmployeeWageBalanceLabelString =>
			(Entity.Employee?.Category == EmployeeCategory.driver
			&& Entity.Employee?.Status == EmployeeStatus.IsWorking)
				? $"{CurrentEmployeeWageString} {FutureEmployeeWageString}"
				: CurrentEmployeeWageString;

		public bool IsNew => UoWGeneric.IsNew;

		public string EmployeeTypeString =>
			IsAdvance
			? "Подотчетное лицо:"
			: "Сотрудник:";

		public bool EmployeeBalanceVisible =>
			(IsAdvance && Entity.Employee?.Category != EmployeeCategory.office)
			|| Entity.TypeOperation == ExpenseType.EmployeeAdvance
			|| Entity.TypeOperation == ExpenseType.Salary;

		public bool OrganisationVisible =>
			Entity.TypeOperation != ExpenseType.Salary
			&& Entity.TypeOperation != ExpenseType.EmployeeAdvance;

		[PropertyChangedAlso(nameof(IsCashExpenceForChangesAdvance))]
		public bool IsAdvance => Entity.TypeOperation == ExpenseType.Advance;


		[PropertyChangedAlso(nameof(CanChangeEmployee))]
		public Employee RestrictEmployee
		{
			get => _restrictEmployee;
			set // init
			{
				if(value != null
					&& _restrictEmployee == null
					&& SetField(ref _restrictEmployee, value))
				{
					Entity.Employee = value;
				}
			}
		}

		public bool CanChangeEmployee => RestrictEmployee == null;

		public bool IsCashExpenceForChangesAdvance =>
			IsAdvance && Entity.ExpenseCategoryId == _financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId;

		public void RefreshCurrentEmployeeWage()
		{
			CurrentEmployeeWage = 0m;

			if(Entity.Employee is null)
			{
				return;
			}

			if(Entity.Employee.Category == EmployeeCategory.driver
				&& Entity.Employee.Status == EmployeeStatus.IsWorking)
			{
				CurrentDriverFutureFinesBalance = _wagesMovementRepository.GetDriverFutureFinesBalance(UoW, Entity.Employee.Id);
			}

			CurrentEmployeeWage = _wagesMovementRepository.GetCurrentEmployeeWageBalance(UoW, Entity.Employee.Id);
		}

		protected override bool BeforeSave()
		{
			Entity.UpdateWagesOperations(UoW);

			if(UoW.IsNew)
			{
				DistributeCash();
			}
			else
			{
				UpdateCashDistributionsDocuments();
			}

			return true;
		}

		public override bool Save(bool close)
		{
			if(base.Save(false))
			{
				if(Entity.RouteListClosing != null)
				{
					_logger.LogInformation("Обновляем сумму долга по МЛ...");
					Entity.RouteListClosing.UpdateRouteListDebt();
					_logger.LogInformation("Ok");
				}

				return base.Save(close);
			}

			return false;
		}

		private void DistributeCash()
		{
			if(Entity.TypeOperation == ExpenseType.Expense
				&& Entity.ExpenseCategoryId == _financialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId)
			{
				_routeListCashOrganisationDistributor.DistributeExpenseCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
			}
			else if(Entity.TypeOperation == ExpenseType.EmployeeAdvance
				|| Entity.TypeOperation == ExpenseType.Salary)
			{
				_expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, Entity, true);
			}
			else
			{
				_expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, Entity);
			}
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

		private void UpdateCashDistributionsDocuments()
		{
			var editor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var document = UoW.Session.QueryOver<CashOrganisationDistributionDocument>()
				.Where(x => x.Expense.Id == Entity.Id)
				.List()
				.FirstOrDefault();

			if(document != null)
			{
				switch(document.Type)
				{
					case CashOrganisationDistributionDocType.ExpenseCashDistributionDoc:
						_expenseCashOrganisationDistributor.UpdateRecords(UoW, (ExpenseCashDistributionDocument)document, Entity, editor);
						break;
					case CashOrganisationDistributionDocType.FuelExpenseCashOrgDistributionDoc:
						_fuelCashOrganisationDistributor.UpdateRecords(UoW, (FuelExpenseCashDistributionDocument)document, Entity, editor);
						break;
				}
			}
		}

		private void Print()
		{
			if(UoWGeneric.HasChanges
				&& (!AskQuestion("Сохранить изменения перед печатью?") || !Save()))
			{
				return;
			}

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Квитанция №{Entity.Id} от {Entity.Date:d}";
			reportInfo.Identifier = "Cash.Expense";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "id", Entity.Id }
			};

			_reportViewOpener.OpenReport(this, reportInfo);
		}

		public void ConfigureForSalaryGiveout(int employeeId, decimal balance)
		{
			Entity.TypeOperation = ExpenseType.Salary;
			RestrictEmployee = UoW.GetById<Employee>(employeeId);
			Entity.Money = balance;
		}

		public void ConfigureForRouteListChangeGiveout(
			int employeeId,
			int routelistId)
		{
			RestrictEmployee = UoW.GetById<Employee>(employeeId);
			Entity.TypeOperation = ExpenseType.Advance;
			Entity.RouteListClosing = UoW.GetById<RouteList>(routelistId);
			AddRouteListInfoToDescription();

			Entity.ExpenseCategoryId = _financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId;
			Entity.Organisation = UoW.GetById<Organization>(_expenseSettings.DefaultChangeOrganizationId);
			Entity.Money = _orderIdsToChanges.Sum(item => item.Value);
		}

		public void CopyFromExpense(int expenseId)
		{
			var source = UoW.GetById<Expense>(expenseId);

			Entity.TypeOperation = source.TypeOperation;
			Entity.ExpenseCategoryId = source.ExpenseCategoryId;
			Entity.Description = source.Description;
			Entity.RelatedToSubdivision = source.RelatedToSubdivision;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.TypeOperation)
				|| e.PropertyName == nameof(Entity.Employee))
			{
				RefreshCurrentEmployeeWage();

				OnPropertyChanged(nameof(CurrentEmployeeWageBalanceLabelString));

				if(!IsAdvance && Entity.RouteListClosing != null)
				{
					Entity.RouteListClosing = null;

					return;
				}

				if(IsCashExpenceForChangesAdvance)
				{
					if(IsDriverCanGetChangeAdvance())
					{
						AddRouteListInfoToDescription();
						Money = _orderIdsToChanges.Sum(item => item.Value);
					}
					else
					{
						Entity.RouteListClosing = null;
					}
				}

				return;
			}

			if(e.PropertyName == nameof(Entity.RouteListClosing))
			{
				if(Entity.RouteListClosing?.Driver != null)
				{
					Entity.Employee = Entity.RouteListClosing?.Driver;
				}

				if(Entity.RouteListClosing != null)
				{
					Money = _orderIdsToChanges.Sum(item => item.Value);
				}
			}
		}

		private bool IsDriverCanGetChangeAdvance()
		{
			if(!IsCashExpenceForChangesAdvance)
			{
				return false;
			}

			if(Entity.RouteListClosing == null || Entity.RouteListClosing.Driver == null)
			{
				return false;
			}

			var unclosedChangeAdvances = _accountableDebtsRepository.GetUnclosedAdvances(
					UoW,
					Entity.RouteListClosing.Driver,
					_financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId,
					null);

			if(unclosedChangeAdvances.Count() > 0)
			{
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error,
					"Закройте сначала ранее выданные авансы со статусом \"Сдача клиенту\"", "Нельзя выдать сдачу");
				return false;
			}


			if(!_orderIdsToChanges.Any())
			{
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Info,
					"Для данного МЛ нет наличных заказов требующих сдачи");
				return false;
			}

			return true;
		}

		private void AddRouteListInfoToDescription()
		{
			if(!IsCashExpenceForChangesAdvance && !string.IsNullOrWhiteSpace(Entity.Description))
			{
				return;
			}

			if(Entity.RouteListClosing != null)
			{
				var routeListInfoText =
					$"Сдача по МЛ №{Entity.RouteListClosing.Id}\n" +
					"-----\n" +
					string.Join(
						"\n",
						_orderIdsToChanges.Select(pair => $"Заказ №{pair.Key} - {pair.Value}руб."));

				Entity.Description =
					string.IsNullOrEmpty(Entity.Description)
					? routeListInfoText
					: $"{Entity.Description}\n{routeListInfoText}";
			}
		}

		private IDictionary<int, decimal> _orderIdsToChanges => Entity.RouteListClosing?.GetCashChangesForOrders() ?? new Dictionary<int, decimal>();
	}
}
