using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.ViewModels.ViewModels.WageCalculation
{
	public partial class WageDistrictLevelRatesAssigningViewModel : DialogTabViewModelBase, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly ILogger<WageDistrictLevelRatesAssigningViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<RouteList> _routeListRepository;
		private readonly IRouteListsWageController _routeListsWageController;
		private readonly IGuiDispatcher _guiDispatcher;
		private EmployeeCategory? _category;
		private IObservableList<EmployeeSelectableNode> _employeeNodes = new ObservableList<EmployeeSelectableNode>();
		private DateTime? _startDate;
		private WageDistrictLevelRates _wageDistrictLevelRatesFilter;
		private WageDistrictLevelRates _wageDistrictLevelRatesForDriverCars;
		private WageDistrictLevelRates _wageDistrictLevelRatesForCompanyCars;
		private WageDistrictLevelRates _wageDistrictLevelRatesForRaskatCars;
		private bool _isExcludeSelectedInFilterWageDistrictLevelRates;
		private bool _isUpdating;
		private CancellationTokenSource _cancellationTokenSource;

		public WageDistrictLevelRatesAssigningViewModel(
			ILogger<WageDistrictLevelRatesAssigningViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IWageCalculationRepository wageCalculationRepository,
			IEmployeeRepository employeeRepository,
			IGenericRepository<RouteList> routeListRepository,
			IRouteListsWageController routeListsWageController,
			IGuiDispatcher guiDispatcher
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService =
				interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_wageCalculationRepository =
				wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_employeeRepository =
				employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListsWageController =
				routeListsWageController ?? throw new ArgumentNullException(nameof(routeListsWageController));
			_guiDispatcher =
				guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			Title = "Привязка ставок";

			WageLevels = _wageCalculationRepository.AllLevelRates(UoW).OrderByDescending(x => x.Id).ToList();
			AvailableCategories = new[] { EmployeeCategory.driver, EmployeeCategory.forwarder };

			SelectAllEmployeesCommand = new DelegateCommand(SelectAllEmployees);
			UnselectAllEmployeesCommand = new DelegateCommand(UnselectAllEmployees);
			UpdateWageDistrictLevelRatesCommand = new DelegateCommand(async () => await UpdateWageDistrictLevelRates(), () => CanUpdateWageDistrictLevelRates);
			UpdateWageDistrictLevelRatesCommand.CanExecuteChangedWith(this, x => x.CanUpdateWageDistrictLevelRates);

			EmployeeNodes.ContentChanged += OnEmployeeNodesContentChanged;

			UpdateEmployeeNodes();
		}

		public DelegateCommand SelectAllEmployeesCommand { get; }
		public DelegateCommand UnselectAllEmployeesCommand { get; }
		public DelegateCommand UpdateWageDistrictLevelRatesCommand { get; }

		public IList<WageDistrictLevelRates> WageLevels { get; }

		public IEnumerable<EmployeeCategory> AvailableCategories { get; }

		public EmployeeCategory? Category
		{
			get => _category;
			set => SetField(ref _category, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public IObservableList<EmployeeSelectableNode> EmployeeNodes
		{
			get => _employeeNodes;
			set => SetField(ref _employeeNodes, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public WageDistrictLevelRates WageDistrictLevelRatesFilter
		{
			get => _wageDistrictLevelRatesFilter;
			set => SetField(ref _wageDistrictLevelRatesFilter, value);
		}

		public bool IsExcludeSelectedInFilterWageDistrictLevelRates
		{
			get => _isExcludeSelectedInFilterWageDistrictLevelRates;
			set => SetField(ref _isExcludeSelectedInFilterWageDistrictLevelRates, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public WageDistrictLevelRates WageDistrictLevelRatesForDriverCars
		{
			get => _wageDistrictLevelRatesForDriverCars;
			set => SetField(ref _wageDistrictLevelRatesForDriverCars, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public WageDistrictLevelRates WageDistrictLevelRatesForCompanyCars
		{
			get => _wageDistrictLevelRatesForCompanyCars;
			set => SetField(ref _wageDistrictLevelRatesForCompanyCars, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public WageDistrictLevelRates WageDistrictLevelRatesForRaskatCars
		{
			get => _wageDistrictLevelRatesForRaskatCars;
			set => SetField(ref _wageDistrictLevelRatesForRaskatCars, value);
		}

		[PropertyChangedAlso(nameof(CanUpdateWageDistrictLevelRates))]
		public bool IsUpdating
		{
			get => _isUpdating;
			set => SetField(ref _isUpdating, value);
		}

		public bool CanUpdateWageDistrictLevelRates =>
			EmployeeNodes.Any(e => e.IsSelected)
			&& StartDate != null
			&& WageDistrictLevelRatesForDriverCars != null
			&& WageDistrictLevelRatesForCompanyCars != null
			&& WageDistrictLevelRatesForRaskatCars != null
			&& !IsUpdating;

		public bool AskSaveOnClose => false;

		private void UpdateEmployeeNodes()
		{
			EmployeeNodes.Clear();

			var employeeNodes = GetEmployeeNodes();

			foreach(var node in employeeNodes)
			{
				EmployeeNodes.Add(node);
			}
		}

		private IList<EmployeeSelectableNode> GetEmployeeNodes()
		{
			var wageDistrictLevelRatesIdFilter = WageDistrictLevelRatesFilter?.Id;

			var query =
				from employee in UoW.Session.Query<Employee>()

				let lastWageLevelRatesIdHavingRequiredParameterItem =
					(int?)(from wageParameter in UoW.Session.Query<EmployeeWageParameter>()
						   join wpi in UoW.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItem.Id equals wpi.Id into wpis
						   from wageParameterItem in wpis.DefaultIfEmpty()
						   join wpicc in UoW.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItemForOurCars.Id equals wpicc.Id into wpiccs
						   from wageParameterItemCompanyCar in wpiccs.DefaultIfEmpty()
						   join wpirc in UoW.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItemForRaskatCars.Id equals wpirc.Id into wpircs
						   from wageParameterItemRaskatCar in wpircs.DefaultIfEmpty()
						   where
						   wageParameter.Employee.Id == employee.Id
						   && wageParameter.EndDate == null
						   && (wageParameterItem.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter
								|| wageParameterItemCompanyCar.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter
								|| wageParameterItemRaskatCar.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter)
						   orderby wageParameter.Id descending
						   select wageParameter.Id)
					.FirstOrDefault()

				where
					AvailableCategories.Contains(employee.Category)
					&& (Category == null || employee.Category == Category)
					&& (wageDistrictLevelRatesIdFilter == null
						|| (IsExcludeSelectedInFilterWageDistrictLevelRates && lastWageLevelRatesIdHavingRequiredParameterItem == null)
						|| (!IsExcludeSelectedInFilterWageDistrictLevelRates && lastWageLevelRatesIdHavingRequiredParameterItem != null))

				orderby employee.LastName
				orderby employee.Name
				orderby employee.Patronymic

				select new EmployeeSelectableNode
				{
					Id = employee.Id,
					LastName = employee.LastName,
					Name = employee.Name,
					Patronymic = employee.Patronymic,
					IsSelected = false
				};

			return query.ToList();
		}

		private void SelectAllEmployees()
		{
			foreach(var emp in EmployeeNodes)
			{
				emp.IsSelected = true;
			}
		}

		private void UnselectAllEmployees()
		{
			foreach(var emp in EmployeeNodes)
			{
				emp.IsSelected = false;
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if(propertyName == nameof(Category)
				|| propertyName == nameof(WageDistrictLevelRatesFilter)
				|| propertyName == nameof(IsExcludeSelectedInFilterWageDistrictLevelRates))
			{
				UpdateEmployeeNodes();
			}
			base.OnPropertyChanged(propertyName);
		}

		private async Task UpdateWageDistrictLevelRates()
		{
			if(IsUpdating || _cancellationTokenSource != null)
			{
				_logger.LogWarning("Обновление отменено. В данный момент уже выполняется обновление расчета з/п");

				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В данный момент уже выполняется обновление расчета з/п");
			}

			if(!CanUpdateWageDistrictLevelRates)
			{
				_logger.LogWarning("Обновление отменено. Не все параметры нового расчета з/п были выбраны");
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Не все параметры нового расчета з/п были выбраны");
				return;
			}

			if(!_interactiveService.Question(
				"Будет обновлен расчет з/п" +
				"\nУказанным сотрудникам будут установлены выбранные уровни ставок" +
				"\nПродолжить?"))
			{
				_logger.LogWarning("Обновление отменено по запросу сотрудника");
				return;
			}

			IsUpdating = true;
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			var employeesRouteListsToUpdateWage = GetEmployeesRouteListsToUpdateWage();

			_logger.LogInformation(
				"Обновления расчета з/п сотрудникам" +
				"Дата начала: {StartDate}, " +
				"Количество выбранных сотрудников: {SelectedEmployeesCount}" +
				"Уровни ставок: " +
				"Авто водителя: Id={WageDistrictLevelRatesForDriverCarsId}, " +
				"Авто компании: Id={WageDistrictLevelRatesForCompanyCarsId}, " +
				"Авто в раскате: Id={WageDistrictLevelRatesForRaskatCarsId}",
				StartDate.Value,
				employeesRouteListsToUpdateWage.Count(),
				WageDistrictLevelRatesForDriverCars.Id,
				WageDistrictLevelRatesForCompanyCars.Id,
				WageDistrictLevelRatesForRaskatCars.Id);

			var isSuccsessful = false;

			try
			{
				await Task.Run(async () =>
				{
					foreach(var employee in employeesRouteListsToUpdateWage)
					{
						await ChangeEmployeeWageParameter(employee.Employee, cancellationToken);

						if(employee.RouteLists.Any())
						{
							await RecalculateEmployeeRouteListsWage(employee.RouteLists, cancellationToken);
						}
					}
				},
				cancellationToken);

				await UoW.CommitAsync();

				isSuccsessful = true;

				_logger.LogInformation("Обновление расчета з/п выбранных сотрудников выполнено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка обновления расчета з/п");
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					IsUpdating = false;

					var message = 
						isSuccsessful
						? "Обновление расчета з/п выбранных сотрудников выполнено успешно"
						: "Обновление расчета з/п выбранных сотрудников завершилось с ошибкой";

					_interactiveService.ShowMessage(
						ImportanceLevel.Info,
						message);
				});

				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private IList<(Employee Employee, IEnumerable<RouteList> RouteLists)> GetEmployeesRouteListsToUpdateWage()
		{
			var employeesToRecalculateWage = new List<(Employee Employee, IEnumerable<RouteList> RouteLists)>();
			var selectedEmployeesWageStartDate = GetSelectedEmployeesWageParametersStartDate();
			var selectedEmployeeIds = selectedEmployeesWageStartDate.Select(x => x.Employee.Id).ToList();
			var routeListsByEmployeesAfterStartDate = GetRouteListsAfterStartDateByEmployees(selectedEmployeeIds);

			foreach(var employeeWageStartDate in selectedEmployeesWageStartDate)
			{
				if(employeeWageStartDate.LastWageParameterStartDate >= StartDate)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"Существующий расчет з/п сотрудника {employeeWageStartDate.Employee.Title} имеет дату начала действия {employeeWageStartDate.LastWageParameterStartDate}" +
						$"\nНовый расчет с указанными ставками не может быть установлен" +
						$"\nДанный сотрудник будет пропущен");

					continue;
				}

				var routeListsAfterStartDate = routeListsByEmployeesAfterStartDate[employeeWageStartDate.Employee.Id];

				if(routeListsAfterStartDate.Any())
				{
					var buttonYes = "Да, пересчитать з/п";
					var buttonNo = "Нет, пропустить сотрудника";

					var answer = _interactiveService.Question(
						new[] { buttonYes, buttonNo },
						$"Сотрудник {employeeWageStartDate.Employee.Title} имеет {routeListsAfterStartDate.Count()} МЛ после {StartDate.Value:d}." +
						$"\nВ случае обновления ставок з/п сотрудника, его зарплата в этих МЛ будет пересчитана" +
						$"\nОбновить ставки и расчет з/п в МЛ сотрудника?");

					if(answer == buttonNo)
					{
						_logger.LogInformation(
							"Обновление расчета з/п сотруднику Id={EmployeeId} пропущено по запросу пользователя",
							employeeWageStartDate.Employee.Id);
						continue;
					}
				}

				employeesToRecalculateWage.Add((employeeWageStartDate.Employee, routeListsAfterStartDate));
			}

			return employeesToRecalculateWage;
		}

		private IEnumerable<EmployeeLastWageParameterStartDateNode> GetSelectedEmployeesWageParametersStartDate()
		{
			var selectedNodeIds = EmployeeNodes.Where(x => x.IsSelected).Select(x => x.Id).ToList();
			var employees = _employeeRepository.GetSelectedEmployeesWageParametersStartDate(UoW, selectedNodeIds);
			return employees;
		}

		private ILookup<int, RouteList> GetRouteListsAfterStartDateByEmployees(
			IEnumerable<int> employeeIds) =>
			_routeListRepository.Get(
				UoW,
				rl => employeeIds.Contains(rl.Driver.Id) && rl.Date >= StartDate.Value)
			.ToLookup(x => x.Driver.Id, x => x);

		private async Task ChangeEmployeeWageParameter(Employee employee, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
					"Выполняем обновление расчета з/п сотрудника Id={EmployeeId}",
					employee.Id);

			var lastWageParameter = employee.WageParameters.LastOrDefault();

			if(lastWageParameter.StartDate >= StartDate)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Существующий расчет з/п сотрудника {employee.Title} имеет дату начала действия {lastWageParameter.StartDate}" +
					$"\nНовый расчет с указанными ставками не может быть установлен" +
					$"\nДанный сотрудник будет пропущен");
			}

			var newWageParameter = new EmployeeWageParameter
			{
				Employee = employee,
				StartDate = StartDate.Value,
				WageParameterItem = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = WageDistrictLevelRatesForDriverCars
				},
				WageParameterItemForOurCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = WageDistrictLevelRatesForCompanyCars
				},
				WageParameterItemForRaskatCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = WageDistrictLevelRatesForRaskatCars
				}
			};

			employee.ChangeWageParameter(newWageParameter, StartDate.Value);
			await UoW.SaveAsync(employee, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Обновление расчета з/п сотрудника Id={EmployeeId} выполнено успешно",
				employee.Id);
		}

		private async Task RecalculateEmployeeRouteListsWage(IEnumerable<RouteList> routeLists, CancellationToken cancellationToken)
		{
			_routeListsWageController.RecalculateRouteListsWage(
				UoW,
				routeLists.ToList(),
				cancellationToken);

			foreach(var routeList in routeLists)
			{
				await UoW.SaveAsync(routeList, cancellationToken: cancellationToken);
			}
		}

		public bool CanClose()
		{
			if(IsUpdating)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Идет обновление ставок, дождитесь окончания операции");
				return false;
			}
			return true;
		}

		private void OnEmployeeNodesContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(CanUpdateWageDistrictLevelRates));
		}

		public override void Dispose()
		{
			if(EmployeeNodes != null)
			{
				EmployeeNodes.ContentChanged -= OnEmployeeNodesContentChanged;
			}
			base.Dispose();
		}
	}
}
