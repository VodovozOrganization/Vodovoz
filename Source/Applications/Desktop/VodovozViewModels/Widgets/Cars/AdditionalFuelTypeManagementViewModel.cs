using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Core.Application.Logistics.Fuel;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Dialogs.Fuel;
using VodovozBusiness.Domain.Logistic;

namespace Vodovoz.ViewModels.Widgets.Cars
{
	public class AdditionalFuelTypeManagementViewModel : EntityWidgetViewModelBase<Car>
	{
		private readonly AdditionalFuelTypeManagementService _additionalFuelTypeManagementService;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;

		private FuelType _selectedNewFuelType;
		private AdditionalFuelType _selectedExistingFuelType;

		public AdditionalFuelTypeManagementViewModel(
			Car entity,
			IUnitOfWork uow,
			DialogViewModelBase parentDialog,
			ICommonServices commonServices,
			ViewModelEEVMBuilder<FuelType> fuelTypeEEVMBuilder,
			AdditionalFuelTypeManagementService additionalFuelTypeManagementService
			) : base(entity, commonServices)
		{
			if(parentDialog is null)
			{
				throw new ArgumentNullException(nameof(parentDialog));
			}

			if(fuelTypeEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(fuelTypeEEVMBuilder));
			}

			UoW = uow
				?? throw new ArgumentNullException(nameof(uow));
			_additionalFuelTypeManagementService = additionalFuelTypeManagementService
				?? throw new ArgumentNullException(nameof(additionalFuelTypeManagementService));

			_interactiveService = commonServices.InteractiveService;
			_currentPermissionService = commonServices.CurrentPermissionService;

			FuelTypeViewModel = fuelTypeEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(parentDialog)
				.ForProperty(this, x => x.SelectedNewFuelType)
				.UseViewModelJournalAndAutocompleter<FuelTypeJournalViewModel>()
				.UseViewModelDialog<FuelTypeViewModel>()
				.Finish(); ;

			_additionalFuelTypeManagementService.Initialize(Entity);

			AddFuelTypeCommand = new DelegateCommand(AddFuelType, () => CanAddFuelType);
			AddFuelTypeCommand.CanExecuteChangedWith(this, x => x.CanAddFuelType);

			RemoveFuelTypeCommand = new DelegateCommand(RemoveFuelType, () => CanRemoveFuelType);
			RemoveFuelTypeCommand.CanExecuteChangedWith(this, x => x.CanRemoveFuelType);
		}

		public IEntityEntryViewModel FuelTypeViewModel { get; }

		public DelegateCommand AddFuelTypeCommand { get; }
		public DelegateCommand RemoveFuelTypeCommand { get; }

		public bool CanEditAdditionalFuelTypes =>
			((Entity.Id == 0 && _currentPermissionService.ValidateEntityPermission(typeof(Car)).CanCreate)
				|| _currentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate)
			&& _currentPermissionService.ValidatePresetPermission(LogisticPermissions.Car.CanEditCarCard);

		[PropertyChangedAlso(nameof(CanAddFuelType))]
		public FuelType SelectedNewFuelType
		{
			get => _selectedNewFuelType;
			set => SetField(ref _selectedNewFuelType, value);
		}

		[PropertyChangedAlso(nameof(CanRemoveFuelType))]
		public AdditionalFuelType SelectedExistingFuelType
		{
			get => _selectedExistingFuelType;
			set => SetField(ref _selectedExistingFuelType, value);
		}

		public bool CanAddFuelType =>
			SelectedNewFuelType != null
			&& _additionalFuelTypeManagementService.CanAddFuelType(SelectedNewFuelType);

		public bool CanRemoveFuelType =>
			SelectedExistingFuelType != null
			&& _additionalFuelTypeManagementService.CanRemoveFuelType(SelectedExistingFuelType);

		public void AddFuelType()
		{
			if(SelectedNewFuelType is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Пожалуйста, выберите тип топлива для добавления");
				return;
			}

			var addFuelTypeResult =
				_additionalFuelTypeManagementService.AddFuelType(SelectedNewFuelType);

			if(addFuelTypeResult.IsFailure)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, addFuelTypeResult.GetErrorsString());
			}

			OnPropertyChanged(nameof(CanAddFuelType));
			OnPropertyChanged(nameof(CanRemoveFuelType));
		}

		public void RemoveFuelType()
		{
			if(SelectedExistingFuelType is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Пожалуйста, выберите тип топлива для удаления");
				return;
			}

			var removeFuelTypeResult =
				_additionalFuelTypeManagementService.RemoveFuelType(SelectedExistingFuelType);

			if(removeFuelTypeResult.IsFailure)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, removeFuelTypeResult.GetErrorsString());
			}

			OnPropertyChanged(nameof(CanAddFuelType));
			OnPropertyChanged(nameof(CanRemoveFuelType));
		}
	}
}
