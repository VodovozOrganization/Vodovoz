using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Fuel.FuelCards;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Transport.Reports.IncorrectFuel
{
	public class IncorrectFuelReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<IncorrectFuelReportViewModel> _logger;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly ViewModelEEVMBuilder<Car> _carEEVMBuilder;
		private readonly ViewModelEEVMBuilder<FuelCard> _fuelCardEEVMBuilder;
		private DateTime _startDate;
		private DateTime _endDate;
		private Car _car;
		private FuelCard _fuelCard;
		private bool _isExcludeOfficeWorkers;
		private IList<CarOwnType> _carOwnTypes;
		private IList<CarTypeOfUse> _carTypesOfUse;

		public IncorrectFuelReportViewModel(
			ILogger<IncorrectFuelReportViewModel> logger,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ViewModelEEVMBuilder<Car> carEEVMBuilder,
			ViewModelEEVMBuilder<FuelCard> fuelCardEEVMBuilder)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_carEEVMBuilder = carEEVMBuilder ?? throw new ArgumentNullException(nameof(carEEVMBuilder));
			_fuelCardEEVMBuilder = fuelCardEEVMBuilder ?? throw new ArgumentNullException(nameof(fuelCardEEVMBuilder));

			Title = "Отчет по заправкам некорректным типом топлива";

			CarEntityEntryViewModel = CreateCarEntityEntryViewModel();
			FuelCardEntityEntryViewModel = CreateFuelCardEntityEntryViewModel();

			SetDefaultValuesInFilter();
		}

		public IEntityEntryViewModel CarEntityEntryViewModel { get; }
		public IEntityEntryViewModel FuelCardEntityEntryViewModel { get; }

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public FuelCard FuelCard
		{
			get => _fuelCard;
			set => SetField(ref _fuelCard, value);
		}

		public bool IsExcludeOfficeWorkers
		{
			get => _isExcludeOfficeWorkers;
			set => SetField(ref _isExcludeOfficeWorkers, value);
		}

		public IList<CarTypeOfUse> CarTypesOfUse
		{
			get => _carTypesOfUse;
			set => SetField(ref _carTypesOfUse, value);
		}

		public IList<CarOwnType> CarOwnTypes
		{
			get => _carOwnTypes;
			set => SetField(ref _carOwnTypes, value);
		}

		private void SetDefaultValuesInFilter()
		{
			var yesterdayDate = DateTime.Today.AddDays(-1);

			StartDate = yesterdayDate;
			EndDate = yesterdayDate;
			IsExcludeOfficeWorkers = true;

			_carOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			_carTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>().ToList(); ;
		}

		private IEntityEntryViewModel CreateCarEntityEntryViewModel()
		{
			var viewModel = _carEEVMBuilder
					.SetViewModel(this)
					.SetUnitOfWork(UoW)
					.ForProperty(this, x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<CarViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		private IEntityEntryViewModel CreateFuelCardEntityEntryViewModel()
		{
			var viewModel = _fuelCardEEVMBuilder
					.SetViewModel(this)
					.SetUnitOfWork(UoW)
					.ForProperty(this, x => x.FuelCard)
					.UseViewModelJournalAndAutocompleter<FuelCardJournalViewModel, FuelCardJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<FuelCardViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelCard)).CanUpdate;

			return viewModel;
		}
	}
}
