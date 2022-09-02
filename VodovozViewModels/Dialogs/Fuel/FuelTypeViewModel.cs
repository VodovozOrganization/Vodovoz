using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic;
namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTypeViewModel : EntityTabViewModelBase<FuelType>, IAskSaveOnCloseViewModel
	{
		private DateTime? _selectedDate;
		private FuelPriceVersion _selectedFuelPriceVersions;
		private IFuelPriceVersionsController _fuelVersionsController;
		private decimal _fuelPrice;

		public FuelTypeViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			CanEdit = PermissionResult.CanUpdate 
			          || (PermissionResult.CanCreate && Entity.Id == 0);

			_fuelVersionsController = new FuelPriceVersionsController(Entity);

			CanReadFuel = CanCreateFuel = CanEditFuel = true;
			FuelPrice = Entity.FuelPriceVersions.FirstOrDefault()?.FuelPrice ?? 0;
			if (FuelPrice == 0)
			{
				FuelPrice = Entity.Cost;
			}
		}

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

		public virtual FuelPriceVersion SelectedFuelPriceVersions
		{
			get => _selectedFuelPriceVersions;
			set
			{
				if(SetField(ref _selectedFuelPriceVersions, value))
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
			&& SelectedFuelPriceVersions != null
			&& _fuelVersionsController.IsValidDateForVersionStartDateChange(SelectedFuelPriceVersions, SelectedDate.Value);

		public void AddNewCarFuelVersion()
		{
			if(SelectedDate == null)
			{
				return;
			}
			_fuelVersionsController.CreateAndAddVersion(FuelPrice, SelectedDate);

			OnPropertyChanged(nameof(CanAddNewFuelVersion));
			OnPropertyChanged(nameof(CanChangeFuelVersionDate));
		}

		public void ChangeFuelVersionStartDate()
		{

			if(SelectedDate == null)
			{
				return;
			}
			_fuelVersionsController.ChangeVersionStartDate(SelectedFuelPriceVersions, SelectedDate.Value);

			OnPropertyChanged(nameof(CanAddNewFuelVersion));
			OnPropertyChanged(nameof(CanChangeFuelVersionDate));
		}
	}
}
