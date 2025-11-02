using Autofac;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Cash
{
	public class IncomeViewModel : EntityTabViewModelBase<Income>
	{
		private readonly ILogger<IncomeViewModel> _logger;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IAccountableDebtsRepository _accountableDebtsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly ISubdivisionRepository _subdivisionsRepository;
		private readonly IRouteListCashOrganisationDistributor _routeListCashOrganisationDistributor;
		private readonly IIncomeCashOrganisationDistributor _incomeCashOrganisationDistributor;
		private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
		private readonly IUserService _userService;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IIncomeSettings _incomeSettings;
		private readonly IPermissionResult _entityPermissionResult;

		private readonly List<SelectableNode<Expense>> _selectableAdvances = new List<SelectableNode<Expense>>();
		private readonly IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> _financialExpenseCategoryNodeInMemoryCacheRepository;
		private readonly IReportInfoFactory _reportInfoFactory;
		private IEntityEntryViewModel _clientViewModel;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;
		private bool _noClose;

		public delegate void DebtsChangedHandler(bool isListReloaded = false, bool isSelectionChanged = false);
		public event DebtsChangedHandler OnDebtsChanged;

		public IncomeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILogger<IncomeViewModel> logger,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			ICategoryRepository categoryRepository,
			IAccountableDebtsRepository accountableDebtsRepository,
			ICounterpartyRepository counterpartyRepository,
			ISubdivisionRepository subdivisionsRepository,
			IRouteListCashOrganisationDistributor routeListCashOrganisationDistributor,
			IIncomeCashOrganisationDistributor incomeCashOrganisationDistributor,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IUserService userService,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IFinancialIncomeCategoriesRepository financialIncomeCategoriesRepository,
			IReportViewOpener reportViewOpener,
			ILifetimeScope lifetimeScope,
			IIncomeSettings incomeSettings,
			IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> domainEntityNodeInMemoryCacheRepository,
			IReportInfoFactory reportInfoFactory
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			if(financialIncomeCategoriesRepository is null)
			{
				throw new ArgumentNullException(nameof(financialIncomeCategoriesRepository));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_categoryRepository = categoryRepository
				?? throw new ArgumentNullException(nameof(categoryRepository));
			_accountableDebtsRepository = accountableDebtsRepository
				?? throw new ArgumentNullException(nameof(accountableDebtsRepository));
			_counterpartyRepository = counterpartyRepository
				?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_subdivisionsRepository = subdivisionsRepository
				?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			_routeListCashOrganisationDistributor = routeListCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(routeListCashOrganisationDistributor));
			_incomeCashOrganisationDistributor = incomeCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(incomeCashOrganisationDistributor));
			_entityExtendedPermissionValidator = entityExtendedPermissionValidator
				?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			_userService = userService
				?? throw new ArgumentNullException(nameof(userService));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings
				?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_financialExpenseCategoryNodeInMemoryCacheRepository = domainEntityNodeInMemoryCacheRepository
				?? throw new ArgumentNullException(nameof(domainEntityNodeInMemoryCacheRepository));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_reportViewOpener = reportViewOpener
				?? throw new ArgumentNullException(nameof(reportViewOpener));
			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));
			_incomeSettings = incomeSettings ?? throw new ArgumentNullException(nameof(incomeSettings));
			_entityPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Income));

			CanEditRectroactively =
				_entityExtendedPermissionValidator.Validate(
					typeof(Income), userService.CurrentUserId, nameof(RetroactivelyClosePermission));

			CanEditDate = commonServices.CurrentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.Income.CanEditDate);

			CachedOrganizations = UoW.GetAll<Organization>().ToList().AsReadOnly();

			if(IsNew)
			{
				Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(Entity.Casher == null)
				{
					InitializationFailed("Ошибка",
						  "Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
					return;
				}

				if(!CanCreate)
				{
					InitializationFailed("Ошибка",
						 "Отсутствуют права на создание приходного ордера");
					return;
				}

				Entity.Organisation = CachedOrganizations
					.Where(x => x.Id == _incomeSettings.DefaultIncomeOrganizationId)
					.FirstOrDefault();

				Entity.Date = DateTime.Now;
			}

			EmployeeViewModel = BuildEmployeeEntityViewModel();

			CashierViewModel = BuildCashierEntityViewModel();

			RouteListViewModel = BuildRouteListEntitiViewModel();

			FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			FinancialIncomeCategoryViewModel = BuildFinancialIncomeCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.IncomeCategoryId,
				() => FinancialIncomeCategory);

			SetPropertyChangeRelation(
				e => e.Id,
				() => IsNew);

			SetPropertyChangeRelation(
				e => e.Date,
				() => CanEdit);

			SetPropertyChangeRelation(
				e => e.Money,
				() => Money);

			SetPropertyChangeRelation(
				e => e.NoFullCloseMode,
				() => NoClose,
				() => CanChangeMoney);

			SetPropertyChangeRelation(
				e => e.TypeOperation,
				() => IsReturnOperation,
				() => IsPayment,
				() => IsNotReturnOperation,
				() => IsReturnOperationOrNew,
				() => IsDriverReport,
				() => ShowRouteList,
				() => CanChangeMoney);

			Entity.PropertyChanged += OnEntityPropertyChanged;

			FillDebtsCommand = new DelegateCommand(UpdateDebts);
			PrintCommand = new DelegateCommand(Print, () => IsReturnOperation);
			SaveCommand = new DelegateCommand(SaveAndClose, () => CanEdit);
			CloseCommand = new DelegateCommand(() => Close(true, CloseSource.Self));

			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
			ValidationContext.ServiceContainer.AddService(typeof(IFinancialIncomeCategoriesRepository), financialIncomeCategoriesRepository);
		}

		public IReadOnlyCollection<Organization> CachedOrganizations { get; }

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => this.GetIdRefField(ref _financialIncomeCategory, Entity.IncomeCategoryId);
			set => this.SetIdRefField(SetField, ref _financialIncomeCategory, () => Entity.IncomeCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region Commands

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		public DelegateCommand FillDebtsCommand { get; }

		public DelegateCommand PrintCommand { get; }

		#endregion Commands

		#region EntityEntry ViewModels

		public IEntityEntryViewModel EmployeeViewModel { get; }

		private IEntityEntryViewModel BuildEmployeeEntityViewModel()
		{
			var employeeEntryViewModelBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, NavigationManager, _lifetimeScope);

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

		private IEntityEntryViewModel BuildCashierEntityViewModel()
		{
			var cashierEntryViewModelBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, NavigationManager, _lifetimeScope);

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
			var routeListEntryViewModelBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = routeListEntryViewModelBuilder
				.ForProperty(x => x.RouteListClosing)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictedByStatuses = new[]
						{
							RouteListStatus.EnRoute,
							RouteListStatus.OnClosing
						};
					})
				.Finish();

			return viewModel;
		}

		public IEntityEntryViewModel ClientViewModel
		{
			get => _clientViewModel;
			set // при обновлении версии языка - заменить на init или при смене countarparty на mvvm - заменить билдер
			{
				if(_clientViewModel is null)
				{
					_clientViewModel = value;
				}
			}
		}

		public IEntityEntryViewModel FinancialIncomeCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialIncomeCategoryViewModel()
		{
			var financialIncomeCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			return financialIncomeCategoryViewModelEntryViewModelBuilder
				.ForProperty(x => x.FinancialIncomeCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Income;
						filter.TargetDocument = Entity.TypeDocument.ToTargetDocument();
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
					})
				.Finish();
		}

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialExpenseCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

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

		public bool NoClose
		{
			get => Entity.NoFullCloseMode;
			set => Entity.NoFullCloseMode = value;
		}

		public decimal Money
		{
			get => Entity.Money;
			set => Entity.Money = value;
		}

		public bool CanChangeMoney => IsNotReturnOperation || NoClose;

		public string CurrencySymbol => NumberFormatInfo.CurrentInfo.CurrencySymbol;

		[PropertyChangedAlso(nameof(IsReturnOperationOrNew))]
		public bool IsNew => UoWGeneric.IsNew;

		public ILifetimeScope Scope => _lifetimeScope; // убрать при обновлении Counterparty на MVVM

		public bool CanEditRectroactively { get; }

		public bool CanEditDate { get; }

		public bool CanCreate => _entityPermissionResult.CanCreate;

		public bool CanEdit => (UoW.IsNew && CanCreate)
			|| (_entityPermissionResult.CanUpdate && Entity.Date.Date == DateTime.Now.Date)
			|| CanEditRectroactively;

		public bool CanChangeRouteList =>
			CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanDelete)
			&& IsDriverReport;

		public bool IsReturnOperation => Entity.TypeOperation == IncomeType.Return;

		public bool IsReturnOperationOrNew => IsReturnOperation && IsNew;

		public bool IsNotReturnOperation => !IsReturnOperation;

		public bool IsPayment => Entity.TypeOperation == IncomeType.Payment;

		public bool IsDriverReport => Entity.TypeOperation == IncomeType.DriverReport;

		public bool ShowRouteList => IsDriverReport || IsReturnOperation;

		public List<SelectableNode<Expense>> SelectableAdvances => _selectableAdvances;

		private Subdivision GetSubdivision(RouteList routeList)
		{
			var user = _userService.GetCurrentUser();
			var employee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var subdivisions = _subdivisionsRepository
				.GetCashSubdivisionsAvailableForUser(UoW, user).ToList();

			if(subdivisions.Any(x => x.Id == employee.Subdivision.Id))
			{
				return employee.Subdivision;
			}

			if(routeList.ClosingSubdivision != null)
			{
				return routeList.ClosingSubdivision;
			}

			throw new InvalidOperationException("Невозможно подобрать подразделение кассы. " +
				"Возможно документ сохраняет не кассир или не правильно заполнены части города в МЛ.");
		}

		private void DistributeCash()
		{
			if(IsDriverReport
				&& Entity.IncomeCategoryId == _financialCategoriesGroupsSettings.RouteListClosingFinancialIncomeCategoryId)
			{
				_routeListCashOrganisationDistributor
					.DistributeIncomeCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
			}
			else if(Entity.TypeOperation == IncomeType.Return)
			{
				_incomeCashOrganisationDistributor
					.DistributeCashForIncome(UoW, Entity, Entity.Organisation);
			}
			else
			{
				_incomeCashOrganisationDistributor.DistributeCashForIncome(UoW, Entity);
			}
		}

		private void UpdateCashDistributionsDocuments()
		{
			var editor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var document = UoW.Session.QueryOver<CashOrganisationDistributionDocument>()
				.Where(x => x.Income.Id == Entity.Id)
				.List()
				.FirstOrDefault();

			if(document != null)
			{
				switch(document.Type)
				{
					case CashOrganisationDistributionDocType.IncomeCashDistributionDoc:
						_incomeCashOrganisationDistributor.UpdateRecords(UoW, (IncomeCashDistributionDocument)document, Entity, editor);
						break;
					case CashOrganisationDistributionDocType.RouteListItemCashDistributionDoc:
						_routeListCashOrganisationDistributor.UpdateIncomeCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
						break;
				}
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

		private void UpdateDebts()
		{
			if(Entity.TypeOperation == IncomeType.Return && Entity.Employee != null)
			{
				var advances =
					_accountableDebtsRepository.GetUnclosedAdvances(
						UoW,
						Entity.Employee,
						Entity.ExpenseCategoryId,
						Entity.Organisation?.Id);

				ClearDebts();

				_financialExpenseCategoryNodeInMemoryCacheRepository
					.WarmUpCacheWithIds(advances
						.Where(x => x.ExpenseCategoryId != null)
						.Select(x => x.ExpenseCategoryId.Value));

				SelectableAdvances.AddRange(
					advances.Select(advance => SelectableNode<Expense>.Create(advance)));

				SelectableAdvances
					.ForEach(advance => advance.SelectChanged += OnAdvanceSelectionChanged);

				OnDebtsChanged?.Invoke(isListReloaded: true);
			}
			else
			{
				ClearDebts();
			}
		}

		private void ClearDebts()
		{
			if(!NoClose)
			{
				Money = 0m;
			}

			if(SelectableAdvances.Any())
			{
				SelectableAdvances
					.ForEach(advance => advance.SelectChanged -= OnAdvanceSelectionChanged);

				SelectableAdvances.Clear();

				OnDebtsChanged?.Invoke(isListReloaded: true);
			}
		}

		public void FillForRoutelist(int routelistId)
		{
			var cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(cashier == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете закрыть МЛ, так как некого указывать в качестве кассира.");
				return;
			}

			var rl = UoW.GetById<RouteList>(routelistId);

			Entity.IncomeCategoryId = _financialCategoriesGroupsSettings.RouteListClosingFinancialIncomeCategoryId;
			Entity.TypeOperation = IncomeType.DriverReport;
			Entity.Date = DateTime.Now;
			Entity.Casher = cashier;
			Entity.Employee = rl.Driver;
			Entity.Description = $"Закрытие МЛ №{rl.Id} от {rl.Date:d}";
			Entity.RouteListClosing = rl;
			Entity.RelatedToSubdivision = GetSubdivision(rl);
		}

		private void NoCloseChangedHandler()
		{
			if(SelectableAdvances == null)
			{
				return;
			}

			if(NoClose && SelectableAdvances.Count(x => x.Selected) > 1)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Частично вернуть можно только один аванс.");

				NoClose = false;
				return;
			}

			if(!NoClose)
			{
				Money = SelectableAdvances.Where(x => x.Selected).Sum(x => x.Value.UnclosedMoney);
			}
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.TypeOperation))
			{
				OnPropertyChanged(nameof(CanChangeRouteList));
				
				Entity.Money = 0m;

				if(IsDriverReport)
				{
					Entity.IncomeCategoryId = _financialCategoriesGroupsSettings.DriverReportFinancialIncomeCategoryId;
				}
			}

			if(e.PropertyName == nameof(Entity.Employee)
				|| e.PropertyName == nameof(Entity.ExpenseCategoryId)
				|| e.PropertyName == nameof(Entity.Organisation)
				|| e.PropertyName == nameof(Entity.TypeOperation))
			{
				FillDebtsCommand.Execute();
			}
		}

		public string GetCachedExpenseCategoryTitle(int id) =>
			_financialExpenseCategoryNodeInMemoryCacheRepository.GetTitleById(id);

		public void ConFigureForReturnChange(int routeListId)
		{
			Entity.TypeOperation = IncomeType.Return;

			var routeList = UoW.GetById<RouteList>(routeListId);

			Entity.Description = $"Приход по МЛ №{routeListId} от {routeList.Date:d}";

			if(routeList is null)
			{
				_logger.LogError("Конфигурация возврата прервана, МЛ {RouteListId} не найден", routeListId);
				InitializationFailed("Ошибка", $"Не найден МЛ {routeListId}");
				return;
			}

			Entity.RouteListClosing = routeList;
			Entity.Employee = routeList.Driver;
			Entity.ExpenseCategoryId = _financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId;

			var unclosedSelectableExpense = _selectableAdvances
				.Where(ex =>
					ex.Value.AdvanceClosed == false
					&& ex.Value.TypeOperation == ExpenseType.Advance
					&& ex.Value.ExpenseCategoryId == _financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId
					&& ex.Value.RouteListClosing != null
					&& ex.Value.RouteListClosing.Id == routeListId)
				.FirstOrDefault();

			if(unclosedSelectableExpense is null)
			{
				_logger.LogError("Не найдены подходящие авансы для возврвта сдачи, для МЛ {RouteLsitId}", routeListId);
				InitializationFailed("Нельзя выполнить возврат сдачи",
					 "Для данного маршрутного листа отсутствуют авансы со статусом \"Сдача клиенту\"");
				return;
			}

			if(unclosedSelectableExpense.Value.Employee is null)
			{
				var errorMessage = "Аванс без сотрудника. Для него нельзя открыть диалог возврата.";
				_logger.LogError(errorMessage);
				InitializationFailed("Ошибка", errorMessage);
				return;
			}

			Entity.Organisation = unclosedSelectableExpense.Value.Organisation;

			unclosedSelectableExpense.Selected = true;
		}

		public void ConfigureForReturn(int expenseId)
		{
			var expense = UoW.GetById<Expense>(expenseId);

			if(expense.Employee == null)
			{
				var errorMessage = "Аванс без сотрудника. Для него нельзя открыть диалог возврата.";
				_logger.LogError(errorMessage);
				InitializationFailed("Ошибка", errorMessage);
				return;
			}

			Entity.TypeOperation = IncomeType.Return;
			Entity.ExpenseCategoryId = expense.ExpenseCategoryId;
			Entity.Employee = expense.Employee;
			Entity.Organisation = expense.Organisation;
			SelectableAdvances.Find(x => x.Value.Id == expenseId).Selected = true;
		}

		protected void OnAdvanceSelectionChanged(object sender, SelectionChanged<Expense> e)
		{
			if(NoClose && (sender as SelectableNode<Expense>).Selected)
			{
				SelectableAdvances
					.ForEach(x =>
					{
						if(x != sender)
						{
							x.SilentUnselect();
						}
					});
			}

			if(NoClose)
			{
				return;
			}

			Money = SelectableAdvances
				.Where(expense => expense.Selected)
				.Sum(selectedExpense => selectedExpense.Value.UnclosedMoney);

			UpdateRouteListInfo();

			OnDebtsChanged?.Invoke(isSelectionChanged: true);
		}

		private void UpdateRouteListInfo()
		{
			if(!(CanChangeRouteList || IsReturnOperation) || !IsNew)
			{
				return;
			}

			var selectedAdvances = SelectableAdvances
				.Where(expense => expense.Selected)
				.Select(e => e.Value.RouteListClosing)
				.ToList();

			var selectedRouteListsCount = selectedAdvances?.GroupBy(rl => rl?.Id).Count();
			if(selectedRouteListsCount != 1)
			{
				Entity.RouteListClosing = null;
				return;
			}

			Entity.RouteListClosing = selectedAdvances.FirstOrDefault();
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
			reportInfo.Identifier = "Cash.ReturnTicket";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "id", Entity.Id }
			};

			_reportViewOpener.OpenReport(this, reportInfo);
		}

		protected override bool BeforeValidation()
		{
			if(Entity.TypeOperation == IncomeType.Return && UoW.IsNew && SelectableAdvances != null)
			{
				Entity.PrepareCloseAdvance(SelectableAdvances
					.Where(x => x.Selected)
					.Select(x => x.Value)
					.ToList());
			}

			return true;
		}

		protected override bool BeforeSave()
		{

			if(Entity.TypeOperation == IncomeType.Return && UoW.IsNew)
			{
				_logger.LogInformation("Закрываем авансы...");
				Entity.CloseAdvances(UoW);
			}

			if(UoW.IsNew)
			{
				_logger.LogInformation("Распределяем финансы...");
				DistributeCash();
			}
			else
			{
				_logger.LogInformation("Обновляем документы распределения финансов...");
				UpdateCashDistributionsDocuments();
			}

			if(Entity.RouteListClosing != null)
			{
				_logger.LogInformation("Обновляем сумму долга по МЛ...");
				Entity.RouteListClosing.UpdateRouteListDebt();
				_logger.LogInformation("Ok");
			}

			return true;
		}
	}
}
