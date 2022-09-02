using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.ComponentModel;
using System.Linq;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewModels.ViewModels.Profitability
{
	public class ProfitabilityConstantsViewModel : TabViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;
		private readonly IEmployeeService _employeeService;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly ICarRepository _carRepository;
		private readonly IProfitabilityConstantsDataViewModelFactory _profitabilityConstantsDataViewModelFactory;
		private readonly IValidator _validator;

		private DelegateCommand _recalculateAndSaveCommand;
		private ProfitabilityConstantsDataViewModel _constantsDataViewModel;
		private Employee _currentEditor;
		
		public ProfitabilityConstantsViewModel(
			IUnitOfWorkGeneric<ProfitabilityConstants> uoWGeneric,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IProfitabilityConstantsRepository profitabilityConstantsRepository,
			IEmployeeService employeeService,
			IWarehouseRepository warehouseRepository,
			ICarRepository carRepository,
			IMonthPickerViewModelFactory monthPickerViewModelFactory,
			IProfitabilityConstantsDataViewModelFactory profitabilityConstantsDataViewModelFactory,
			IValidator validator,
			DateTime? calculatedMonth = null) : base(commonServices?.InteractiveService, navigationManager)
		{
			if(monthPickerViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(monthPickerViewModelFactory));
			}

			UoWGeneric = uoWGeneric ?? throw new ArgumentNullException(nameof(uoWGeneric));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_profitabilityConstantsDataViewModelFactory =
				profitabilityConstantsDataViewModelFactory
					?? throw new ArgumentNullException(nameof(profitabilityConstantsDataViewModelFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			
			Initialize(calculatedMonth, monthPickerViewModelFactory);
			TabName = $"Константы для рентабельности за {Entity.CalculatedMonth:Y}";
		}

		public bool IsCalculationDateAndAuthorActive => !UoW.IsNew;

		public ProfitabilityConstants Entity => UoWGeneric.Root;
		public MonthPickerViewModel MonthPickerViewModel { get; private set; }

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

				CalculateAdministrativeAndWarehouseExpensesConstants();
				CalculateAmortisationConstants();
				CalculateRepairCostConstants();

				Save();
				MonthPickerViewModel.UpdateState();
			}
			));
		
		private DateTime CurrentMonth => MonthPickerViewModel.SelectedMonth;
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
					.Where(x => x.Selected)
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
					.Where(x => x.Selected)
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
		
		private void Initialize(
			DateTime? calculatedMonth,
			IMonthPickerViewModelFactory monthPickerViewModelFactory)
		{
			_currentEditor = _employeeService.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId);

			if(calculatedMonth.HasValue)
			{
				Entity.CalculatedMonth = calculatedMonth.Value;
			}

			MonthPickerViewModel = monthPickerViewModelFactory.CreateNewMonthPickerViewModel(
				calculatedMonth ?? Entity.CalculatedMonth,
				CanSelectNextMonth,
				CanSelectPreviousMonth);

			MonthPickerViewModel.PropertyChanged += OnMonthPickerViewModelPropertyChanged;

			ConstantsDataViewModel = _profitabilityConstantsDataViewModelFactory.CreateProfitabilityConstantsDataViewModel(UoW, Entity);
			OnPropertyChanged(nameof(IsCalculationDateAndAuthorActive));
		}

		private void OnMonthPickerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MonthPickerViewModel.SelectedMonth))
			{
				UpdateData(CurrentMonth);
			}
		}

		private void UpdateData(DateTime currentMonth)
		{
			var calculatedConstants = _profitabilityConstantsRepository.GetProfitabilityConstantsByCalculatedMonth(UoW, currentMonth);
			UoW.Dispose();

			if(calculatedConstants != null)
			{
				CreateNewUoW(calculatedConstants.Id);
			}
			else
			{
				CreateNewUoW(currentMonth);
			}

			TabName = $"Константы для рентабельности за {Entity.CalculatedMonth:Y}";
			ConstantsDataViewModel = _profitabilityConstantsDataViewModelFactory.CreateProfitabilityConstantsDataViewModel(UoW, Entity);
			OnPropertyChanged(nameof(IsCalculationDateAndAuthorActive));
		}

		private void CreateNewUoW(DateTime dateTime)
		{
			UoWGeneric = _unitOfWorkFactory.CreateWithNewRoot<ProfitabilityConstants>();
			UoWGeneric.Root.CalculatedMonth = dateTime;
		}
		
		private void CreateNewUoW(int profitabilityConstantsId)
		{
			UoWGeneric = _unitOfWorkFactory.CreateForRoot<ProfitabilityConstants>(profitabilityConstantsId);
		}

		private bool CanSelectNextMonth(DateTime dateTime)
		{
			if(dateTime.Month == DateTime.Today.Month - 1)
			{
				return false;
			}

			return _profitabilityConstantsRepository.ProfitabilityConstantsByCalculatedMonthExists(UoW, dateTime, dateTime.AddMonths(1));
		}
		
		private bool CanSelectPreviousMonth(DateTime dateTime)
		{
			return _profitabilityConstantsRepository.ProfitabilityConstantsByCalculatedMonthExists(UoW, dateTime.AddMonths(-1), dateTime);
		}

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}
}
