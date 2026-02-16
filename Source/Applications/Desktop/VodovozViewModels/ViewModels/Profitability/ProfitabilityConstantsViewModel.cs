using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.ComponentModel;
using System.Linq;
using NLog;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Vodovoz.Services;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ViewModels.Profitability;

namespace Vodovoz.ViewModels.Profitability
{
	public class ProfitabilityConstantsViewModel : DialogViewModelBase, IDisposable
	{
		private const string _startCalculateConstants = "Начинаем расчет констант...";
		private const string _saveConstants = "Сохраняем константы";
		private const string _startRecalculateRlProfitabilities = "Начинаем перерасчет рентабельностей МЛ...";
		private const string _recalculated = "Завершили перерасчет";
		
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly IProfitabilityConstantsViewModelHandler _profitabilityConstantsViewModelHandler;
		private readonly IUserService _userService;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;
		private readonly IEmployeeService _employeeService;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly ICarRepository _carRepository;
		private readonly IProfitabilityConstantsDataViewModelFactory _profitabilityConstantsDataViewModelFactory;
		private readonly IValidator _validator;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		private DelegateCommand _recalculateAndSaveCommand;
		private ProfitabilityConstantsDataViewModel _constantsDataViewModel;
		private Employee _currentEditor;
		private bool _isIdleState = true;
		
		public ProfitabilityConstantsViewModel(
			IProfitabilityConstantsViewModelHandler profitabilityConstantsViewModelHandler,
			IUserService userService,
			INavigationManager navigationManager,
			IProfitabilityConstantsRepository profitabilityConstantsRepository,
			IEmployeeService employeeService,
			IWarehouseRepository warehouseRepository,
			ICarRepository carRepository,
			IDatePickerViewModelFactory monthPickerViewModelFactory,
			IProfitabilityConstantsDataViewModelFactory profitabilityConstantsDataViewModelFactory,
			IValidator validator,
			IRouteListProfitabilityController routeListProfitabilityController) : base(navigationManager)
		{
			if(monthPickerViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(monthPickerViewModelFactory));
			}
			
			_profitabilityConstantsViewModelHandler =
				profitabilityConstantsViewModelHandler ?? throw new ArgumentNullException(nameof(profitabilityConstantsViewModelHandler));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_profitabilityConstantsDataViewModelFactory =
				profitabilityConstantsDataViewModelFactory
					?? throw new ArgumentNullException(nameof(profitabilityConstantsDataViewModelFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));

			Initialize(monthPickerViewModelFactory);
			Title = $"Константы для рентабельности за {Entity.CalculatedMonth:Y}";
		}
		
		public ProfitabilityConstants Entity => UoWGeneric.Root;
		public DatePickerViewModel MonthPickerViewModel { get; private set; }

		public bool IsIdleState
		{
			get => _isIdleState;
			set => SetField(ref _isIdleState, value);
		}

		public ProfitabilityConstantsDataViewModel ConstantsDataViewModel
		{
			get => _constantsDataViewModel;
			private set => SetField(ref _constantsDataViewModel, value);
		}

		public DelegateCommand RecalculateAndSaveCommand => _recalculateAndSaveCommand ?? (_recalculateAndSaveCommand = new DelegateCommand(
			() =>
			{
				if(!_validator.Validate(Entity))
				{
					return;
				}

				try
				{
					IsIdleState = false;
					MonthPickerViewModel.UpdateState();
					var progress = ConstantsDataViewModel.ProgressBarDisplayable;
					progress.Update(_startCalculateConstants);
					_logger.Debug(_startCalculateConstants);
					progress.Update("Считаем затраты по первому блоку...");
					CalculateAdministrativeAndWarehouseExpensesConstants();
					progress.Update("Считаем затраты по второму блоку...");
					CalculateAmortisationConstants();
					progress.Update("Считаем затраты по третьему блоку...");
					CalculateRepairCostConstants();
					_logger.Debug("Закончили");

					progress.Update(_saveConstants);
					_logger.Debug(_saveConstants);
					Save();

					progress.Update(_startRecalculateRlProfitabilities);
					_logger.Debug(_startRecalculateRlProfitabilities);
					_routeListProfitabilityController.RecalculateRouteListProfitabilitiesByCalculatedMonth(
						UoW, CalculatedMonth, true, progress);
					progress.Update(_recalculated);
					_logger.Debug(_recalculated);

					UoW.Save();
				}
				catch(Exception e)
				{
					_logger.Error(e, "Ошибка при пересчете констант");
					throw;
				}
				finally
				{
					IsIdleState = true;
					ConstantsDataViewModel.FirePropertyChanged(nameof(ConstantsDataViewModel.IsCalculationDateAndAuthorActive));
					MonthPickerViewModel.UpdateState();
				}
			}
			));

		private DateTime CalculatedMonth => MonthPickerViewModel.SelectedDate;
		private IUnitOfWorkGeneric<ProfitabilityConstants> UoWGeneric { get; set; }
		private IUnitOfWork UoW => UoWGeneric;

		private void Save()
		{
			Entity.CalculationSaved = DateTime.Now;
			Entity.CalculationAuthor = _currentEditor;
			
			try
			{
				UoW.Save();
			}
			catch
			{
				Entity.CalculationSaved = null;
				Entity.CalculationAuthor = null;
				throw;
			}
		}

		private void CalculateAdministrativeAndWarehouseExpensesConstants()
		{
			CalculateAdministrativeExpensesConstants();
			CalculateWarehouseExpensesConstants();
		}

		private void CalculateAdministrativeExpensesConstants()
		{
			var selectedAdministrativeExpensesProductGroupsIds =
				ConstantsDataViewModel.AdministrativeExpensesProductGroupsFilterViewModel.Parameters
					.SelectMany(x => x.GetAllSelected())
					.Select(x => (int)x.Value)
					.ToArray();

			var selectedAdministrativeExpensesWarehousesIds =
				ConstantsDataViewModel.AdministrativeExpensesWarehousesFilterViewModel.Parameters
					.Where(x => x.Selected)
					.Select(x => (int)x.Value)
					.ToArray();

			Entity.UpdateAdministartiveExpensesFilters(selectedAdministrativeExpensesProductGroupsIds,
				selectedAdministrativeExpensesWarehousesIds);
			
			Entity.CalculateAdministrativeExpensesPerKg(
				UoW, _warehouseRepository, selectedAdministrativeExpensesProductGroupsIds, selectedAdministrativeExpensesWarehousesIds);
		}
		
		private void CalculateWarehouseExpensesConstants()
		{
			var selectedWarehouseExpensesProductGroupsIds =
				ConstantsDataViewModel.WarehouseExpensesProductGroupsFilterViewModel.Parameters
					.SelectMany(x => x.GetAllSelected())
					.Select(x => (int)x.Value)
					.ToArray();

			var selectedWarehouseExpensesWarehousesIds =
				ConstantsDataViewModel.WarehouseExpensesWarehousesFilterViewModel.Parameters
					.Where(x => x.Selected)
					.Select(x => (int)x.Value)
					.ToArray();

			Entity.UpdateWarehouseExpensesFilters(selectedWarehouseExpensesProductGroupsIds, selectedWarehouseExpensesWarehousesIds);

			Entity.CalculateWarehouseExpensesPerKg(
				UoW, _warehouseRepository, selectedWarehouseExpensesProductGroupsIds, selectedWarehouseExpensesWarehousesIds);
		}

		private void CalculateAmortisationConstants()
		{
			Entity.CalculateAverageMileageForCarsByTypeOfUse(UoW, _profitabilityConstantsRepository);
			Entity.CalculateAmortisation();
		}

		private void CalculateRepairCostConstants()
		{
			var selectedCarEventTypesIds =
				ConstantsDataViewModel.CarEventsFilterViewModel.Parameters
					.Where(x => x.Selected)
					.Select(x => (int)x.Value)
					.ToArray();
			
			Entity.UpdateCarEventTypesFilter(selectedCarEventTypesIds);
			Entity.CalculateOperatingExpenses(UoW, _carRepository, selectedCarEventTypesIds);
			Entity.CalculateRepairCost();
		}
		
		private void Initialize(IDatePickerViewModelFactory monthPickerViewModelFactory)
		{
			UoWGeneric = _profitabilityConstantsViewModelHandler.GetLastCalculatedProfitabilityConstants();
			_currentEditor = _employeeService.GetEmployeeForUser(UoW, _userService.CurrentUserId);

			MonthPickerViewModel = monthPickerViewModelFactory.CreateNewDatePickerViewModel(
				Entity.CalculatedMonth,
				ChangeDateType.Month,
				canSelectNextDateFunc: CanSelectNextMonth,
				canSelectPreviousDateFunc: CanSelectPreviousMonth);

			MonthPickerViewModel.PropertyChanged += OnMonthPickerViewModelPropertyChanged;

			ConstantsDataViewModel = _profitabilityConstantsDataViewModelFactory.CreateProfitabilityConstantsDataViewModel(UoW, Entity);
		}

		private void OnMonthPickerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MonthPickerViewModel.SelectedDate))
			{
				UpdateData(CalculatedMonth);
			}
		}

		private void UpdateData(DateTime calculatedMonth)
		{
			var newUoWGeneric = _profitabilityConstantsViewModelHandler.GetProfitabilityConstantsByCalculatedMonth(UoW, calculatedMonth);
			UoW.Dispose();
			UoWGeneric = newUoWGeneric;

			Title = $"Константы для рентабельности за {Entity.CalculatedMonth:Y}";
			ConstantsDataViewModel = _profitabilityConstantsDataViewModelFactory.CreateProfitabilityConstantsDataViewModel(UoW, Entity);
			ConstantsDataViewModel.FirePropertyChanged(nameof(ConstantsDataViewModel.IsCalculationDateAndAuthorActive));
		}

		private bool CanSelectNextMonth(DateTime dateTime)
		{
			if(dateTime.Month == DateTime.Today.Month - 1 || !IsIdleState)
			{
				return false;
			}

			return _profitabilityConstantsRepository.ProfitabilityConstantsByCalculatedMonthExists(UoW, dateTime, dateTime.AddMonths(1));
		}
		
		private bool CanSelectPreviousMonth(DateTime dateTime)
		{
			return IsIdleState &&
				_profitabilityConstantsRepository.ProfitabilityConstantsByCalculatedMonthExists(UoW, dateTime.AddMonths(-1), dateTime);
		}

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
