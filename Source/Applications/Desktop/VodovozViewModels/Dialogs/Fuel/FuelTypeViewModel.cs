using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTypeViewModel : EntityTabViewModelBase<FuelType>, IAskSaveOnCloseViewModel
	{
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private DateTime? _selectedDate;
		private FuelPriceVersion _selectedFuelPriceVersion;
		private readonly IFuelPriceVersionsController _fuelVersionsController;
		private decimal _fuelPrice;

		public FuelTypeViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFuelRepository fuelRepository,
			IRouteListProfitabilityController routeListProfitabilityController) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			if(fuelRepository is null)
			{
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_fuelVersionsController = new FuelPriceVersionsController();
			_fuelVersionsController.SetFuelType(Entity);

			CanEdit = PermissionResult.CanUpdate
				|| (PermissionResult.CanCreate && Entity.Id == 0);

			CanReadFuel = true;
			var permissionFuelPriceVersionResult = commonServices.PermissionService.ValidateUserPermission(typeof(FuelPriceVersion), commonServices.UserService.CurrentUserId);
			CanCreateFuel = permissionFuelPriceVersionResult.CanCreate;
			CanEditFuel = permissionFuelPriceVersionResult.CanUpdate;

			FuelProductGroups = fuelRepository.GetGazpromFuelProductsGroupsByFuelTypeId(UoW, Entity.Id);
			FuelProductsInGroup = fuelRepository.GetFuelProductsByFuelTypeId(UoW, Entity.Id);
		}

		public IEnumerable<GazpromFuelProductsGroup> FuelProductGroups { get; }

		public IEnumerable<GazpromFuelProduct> FuelProductsInGroup { get; }

		public bool CanEdit { get; }

		public bool AskSaveOnClose => CanEdit;

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					OnPropertyChanged(nameof(CanAddNewFuelVersion));
					OnPropertyChanged(nameof(CanChangeFuelVersionDate));
				}
			}
		}

		public virtual FuelPriceVersion SelectedFuelPriceVersion
		{
			get => _selectedFuelPriceVersion;
			set
			{
				if(SetField(ref _selectedFuelPriceVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeFuelVersionDate));
				}
			}
		}

		public virtual decimal FuelPrice
		{
			get => _fuelPrice;
			set => SetField(ref _fuelPrice, value);
		}

		public virtual bool CanReadFuel { get; }
		public virtual bool CanCreateFuel { get; }
		public virtual bool CanEditFuel { get; }

		public bool CanAddNewFuelVersion => CanCreateFuel
			&& SelectedDate.HasValue
			&& _fuelVersionsController.IsValidDateForNewCarVersion(SelectedDate.Value);

		public bool CanChangeFuelVersionDate => CanEditFuel
			&& SelectedDate.HasValue
			&& SelectedFuelPriceVersion != null
			&& _fuelVersionsController.IsValidDateForVersionStartDateChange(SelectedFuelPriceVersion, SelectedDate.Value);

		public void AddNewCarFuelVersion()
		{
			if(SelectedDate == null)
			{
				return;
			}
			_fuelVersionsController.CreateAndAddVersion(FuelPrice, SelectedDate);
			_routeListProfitabilityController.RecalculateRouteListProfitabilitiesByDate(UoW, SelectedDate.Value, null, Entity);

			OnPropertyChanged(nameof(CanAddNewFuelVersion));
			OnPropertyChanged(nameof(CanChangeFuelVersionDate));
		}

		public void ChangeFuelVersionStartDate()
		{
			if(SelectedDate == null)
			{
				return;
			}
			_fuelVersionsController.ChangeVersionStartDate(SelectedFuelPriceVersion, SelectedDate.Value);
			RecalculateRouteListProfitabilitiesBetweenDates(SelectedFuelPriceVersion.StartDate, SelectedDate.Value);

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
