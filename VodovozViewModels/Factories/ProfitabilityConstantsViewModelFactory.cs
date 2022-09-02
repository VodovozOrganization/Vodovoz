using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Validation;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.ViewModels.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public class ProfitabilityConstantsViewModelFactory
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;
		private readonly IEmployeeService _employeeService;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly ICarRepository _carRepository;
		private readonly MonthPickerViewModelFactory _monthPickerViewModelFactory;
		private readonly ProfitabilityConstantsDataViewModelFactory _profitabilityConstantsDataViewModelFactory;
		private readonly IValidator _validator;

		public ProfitabilityConstantsViewModelFactory(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IProfitabilityConstantsRepository profitabilityConstantsRepository,
			IEmployeeService employeeService,
			IWarehouseRepository warehouseRepository,
			ICarRepository carRepository,
			MonthPickerViewModelFactory monthPickerViewModelFactory,
			ProfitabilityConstantsDataViewModelFactory profitabilityConstantsDataViewModelFactory,
			IValidator validator)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_monthPickerViewModelFactory =
				monthPickerViewModelFactory ?? throw new ArgumentNullException(nameof(monthPickerViewModelFactory));
			_profitabilityConstantsDataViewModelFactory =
				profitabilityConstantsDataViewModelFactory
					?? throw new ArgumentNullException(nameof(profitabilityConstantsDataViewModelFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
		}
		
		public ProfitabilityConstantsViewModel CreateProfitabilityConstantsViewModel()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var lastProfitabilityContants = _profitabilityConstantsRepository.GetLastProfitabilityConstants(uow);
				return lastProfitabilityContants != null ? CreateNewProfitabilityConstantsViewModel(lastProfitabilityContants.Id) : CreateNewProfitabilityConstantsViewModel();
			}
		}

		private ProfitabilityConstantsViewModel CreateNewProfitabilityConstantsViewModel(int? profitabilityConstantsId = null)
		{
			DateTime? calculatedMonth = null;

			if(profitabilityConstantsId == null)
			{
				calculatedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month - 1, 1);
			}
			
			var uow = profitabilityConstantsId.HasValue
				? _unitOfWorkFactory.CreateForRoot<ProfitabilityConstants>(profitabilityConstantsId.Value)
				: _unitOfWorkFactory.CreateWithNewRoot<ProfitabilityConstants>();
			
			return new ProfitabilityConstantsViewModel(
				uow,
				_unitOfWorkFactory,
				_commonServices,
				_navigationManager,
				_profitabilityConstantsRepository,
				_employeeService,
				_warehouseRepository,
				_carRepository,
				_monthPickerViewModelFactory,
				_profitabilityConstantsDataViewModelFactory,
				_validator,
				calculatedMonth);
		}
	}
}
