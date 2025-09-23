using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarModelViewModel : EntityTabViewModelBase<CarModel>, IAskSaveOnCloseViewModel
	{
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private DateTime? _selectedFuelDate;
		private CarFuelVersion _selectedCarFuelVersion;
		private ICarFuelVersionsController _fuelVersionsController;

		public CarModelViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarManufacturerJournalFactory carManufacturerJournalFactory,
			IRouteListProfitabilityController routeListProfitabilityController)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			CarManufacturerJournalFactory = carManufacturerJournalFactory
				?? throw new ArgumentNullException(nameof(carManufacturerJournalFactory));
			_fuelVersionsController = new CarFuelVersionsController(Entity);

			CanReadFuel = true;
			CanCreateFuel = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_car_fuel_version");
			CanEditFuel = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_car_fuel_version_date");
		}

		public double FuelConsumption { get; set; } 
		public ICarManufacturerJournalFactory CarManufacturerJournalFactory { get; }
		public bool CanEdit => PermissionResult.CanUpdate || PermissionResult.CanCreate && Entity.Id == 0;
		public bool AskSaveOnClose => CanEdit;

		public virtual DateTime? SelectedFuelDate
		{
			get => _selectedFuelDate;
			set
			{
				if(SetField(ref _selectedFuelDate, value))
				{
					OnPropertyChanged(nameof(CanAddNewFuelVersion));
					OnPropertyChanged(nameof(CanChangeFuelVersionDate));
				}
			}
		}

		public virtual CarFuelVersion SelectedCarFuelVersion
		{
			get => _selectedCarFuelVersion;
			set
			{
				if(SetField(ref _selectedCarFuelVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeFuelVersionDate));
				}
			}
		}

		public virtual bool CanReadFuel { get; }
		public virtual bool CanCreateFuel { get; }
		public virtual bool CanEditFuel { get; }

		public bool CanAddNewFuelVersion =>	CanCreateFuel 
			&& SelectedFuelDate.HasValue
			&& _fuelVersionsController.IsValidDateForNewCarVersion(SelectedFuelDate.Value);

		public bool CanChangeFuelVersionDate => CanEditFuel 
			&& SelectedFuelDate.HasValue
			&& SelectedCarFuelVersion != null
			&& _fuelVersionsController.IsValidDateForVersionStartDateChange(SelectedCarFuelVersion, SelectedFuelDate.Value);

		public void AddNewCarFuelVersion()
		{
			if(SelectedFuelDate == null)
			{
				return;
			}
			_fuelVersionsController.CreateAndAddVersion(FuelConsumption, SelectedFuelDate);
			_routeListProfitabilityController.RecalculateRouteListProfitabilitiesByDate(UoW, SelectedFuelDate.Value, Entity, null);

			OnPropertyChanged(nameof(CanAddNewFuelVersion));
			OnPropertyChanged(nameof(CanChangeFuelVersionDate));
		}

		public void ChangeFuelVersionStartDate()
		{
			if(SelectedFuelDate == null)
			{
				return;
			}
			_fuelVersionsController.ChangeVersionStartDate(SelectedCarFuelVersion, SelectedFuelDate.Value);
			RecalculateRouteListProfitabilitiesBetweenDates(SelectedCarFuelVersion.StartDate, SelectedFuelDate.Value);
				
			OnPropertyChanged(nameof(CanAddNewFuelVersion));
			OnPropertyChanged(nameof(CanChangeFuelVersionDate));
		}
		
		private void RecalculateRouteListProfitabilitiesBetweenDates(DateTime oldStartDate, DateTime newStartDate)
		{
			if(oldStartDate < newStartDate)
			{
				_routeListProfitabilityController.RecalculateRouteListProfitabilitiesBetweenDates(UoW, oldStartDate, newStartDate);
			}
			else
			{
				_routeListProfitabilityController.RecalculateRouteListProfitabilitiesBetweenDates(UoW, newStartDate, oldStartDate);
			}
		}
	}
}
