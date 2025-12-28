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
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.ViewModels.ViewModels.WageCalculation
{
	public class WageDistrictLevelRatesAssigningViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		private readonly ILogger<WageDistrictLevelRatesAssigningViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
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

		public WageDistrictLevelRatesAssigningViewModel(
			ILogger<WageDistrictLevelRatesAssigningViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IWageCalculationRepository wageCalculationRepository,
			IGenericRepository<Employee> employeeRepository,
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
			_guiDispatcher =
				guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			Title = "Привязка ставок";

			WageLevels = _wageCalculationRepository.AllLevelRates(UoW).ToList();
			AvailableCategories = new[] { EmployeeCategory.driver, EmployeeCategory.forwarder };

			SelectAllEmployeesCommand = new DelegateCommand(SelectAllEmployees);
			UnselectAllEmployeesCommand = new DelegateCommand(UnselectAllEmployees);
			UpdateWageDistrictLevelRatesCommand = new AsyncCommand(_guiDispatcher, UpdateWageDistrictLevelRates, () => CanUpdateWageDistrictLevelRates);
			UpdateWageDistrictLevelRatesCommand.CanExecuteChangedWith(this, x => x.CanUpdateWageDistrictLevelRates);

			UpdateEmployeeNodes();
		}

		public DelegateCommand SelectAllEmployeesCommand { get; }
		public DelegateCommand UnselectAllEmployeesCommand { get; }
		public AsyncCommand UpdateWageDistrictLevelRatesCommand { get; }

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

		public bool CanUpdateWageDistrictLevelRates =>
			EmployeeNodes.Any(e => e.IsSelected)
			&& StartDate != null
			&& (WageDistrictLevelRatesForDriverCars != null
				|| WageDistrictLevelRatesForCompanyCars != null
				|| WageDistrictLevelRatesForRaskatCars != null);

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
			var query =
				from employee in UoW.Session.Query<Employee>()

				where
					((Category != null && employee.Category == Category) || (Category == null && AvailableCategories.Contains(employee.Category)))

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

		private async Task<IEnumerable<Employee>> GetSelectedEmployees(CancellationToken cancellationToken = default)
		{
			var selectedNodeIds = EmployeeNodes.Where(x => x.IsSelected).Select(x => x.Id).ToList();

			var employeesResult = await _employeeRepository.GetAsync(
				UoW,
				e => selectedNodeIds.Contains(e.Id),
				cancellationToken: cancellationToken);

			return employeesResult.Value;
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
			if(propertyName == nameof(Category))
			{
				UpdateEmployeeNodes();
			}
			base.OnPropertyChanged(propertyName);
		}

		private async Task UpdateWageDistrictLevelRates(CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Начало обновления расчета з/п сотрудникам" +
				"Дата начала: {StartDate}, " +
				"Уровни ставок: " +
				"Авто водителя: Id={WageDistrictLevelRatesForDriverCarsId}, " +
				"Авто компании: Id={WageDistrictLevelRatesForCompanyCarsId}, " +
				"Авто в раскате: Id={WageDistrictLevelRatesForRaskatCarsId}",
				StartDate.Value,
				WageDistrictLevelRatesForDriverCars.Id,
				WageDistrictLevelRatesForCompanyCars.Id,
				WageDistrictLevelRatesForRaskatCars.Id);

			if(_isUpdating)
			{
				_logger.LogWarning("Обновление отменено. В данный момент уже выполняется обновление расчета з/п");

				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В данный момент уже выполняется обновление расчета з/п");
			}

			//if(!_interactiveService.Question(
			//	"Будет обновлен расчет з/п" +
			//	"\nУказанным сотрудникам будут установлены выбранные уровни ставок" +
			//	"\nПродолжить?"))
			//{
			//	_logger.LogWarning("Обновление отменено по запросу сотрудника");
			//	return;
			//}

			_isUpdating = true;
			var employees = await GetSelectedEmployees(cancellationToken);

			try
			{
				foreach(var employee in employees)
				{
					_logger.LogInformation(
						"Обновление отменено. В данный момент уже выполняется обновление расчета з/п");
					var lastWageParameter = employee.WageParameters.LastOrDefault();
					if(lastWageParameter.StartDate >= StartDate)
					{
						_interactiveService.ShowMessage(
							ImportanceLevel.Warning,
							$"Существующий расчет з/п сотрудника {employee.Title} имеет дату начала действия {lastWageParameter.StartDate}" +
							$"\nНовый расчет с указанными ставками не может быть установлен" +
							$"\nДанный сотрудник будет пропущен");
					}

					lastWageParameter.EndDate = StartDate.Value.AddDays(-1);

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

					await UoW.SaveAsync(lastWageParameter, cancellationToken: cancellationToken);
					await UoW.SaveAsync(newWageParameter, cancellationToken: cancellationToken);
				}

				await UoW.CommitAsync();

				_logger.LogInformation("Обновление расчета з/п выполнено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка обновления расчета з/п");
			}
			finally
			{
				_isUpdating = false;
			}
		}

		public bool CanClose()
		{
			if(_isUpdating)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Идет обновление ставок, дождитесь окончания операции");
			}
			return _isUpdating;
		}
	}

	public class EmployeeSelectableNode : PropertyChangedBase
	{
		private bool _isSelected;

		public int Id { get; set; }
		public string LastName { get; set; }
		public string Name { get; set; }
		public string Patronymic { get; set; }

		public bool IsSelected
		{
			get => _isSelected;
			set => SetField(ref _isSelected, value);
		}

		public string FullName =>
			LastName + " " + Name + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic);
	}
}
