using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Fuel;
using Vodovoz.ViewModels.Fuel.FuelCards;

namespace Vodovoz.ViewModels.Widgets.Cars
{
	public class FuelCardVersionViewModel : EntityWidgetViewModelBase<Car>, IDisposable
	{
		private DateTime? _selectedDate;
		private FuelCard _selectedFuelCard;
		private FuelCardVersion _selectedVersion;
		private DialogViewModelBase _parentDialog;

		private readonly IFuelCardVersionService _fuelCardVersionController;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IUnitOfWork _unitOfWork;

		public FuelCardVersionViewModel(
			Car entity,
			ICommonServices commonServices,
			IFuelCardVersionService fuelCardVersionController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory)
			: base(entity, commonServices)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_fuelCardVersionController = fuelCardVersionController ?? throw new ArgumentNullException(nameof(fuelCardVersionController));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Версия топливной карты");

			CanRead = PermissionResult.CanRead;
			CanCreate = PermissionResult.CanCreate && Entity.Id == 0
				&& commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeFuelCardNumber);
			CanEdit = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeFuelCardNumber);

			if(IsNewCar)
			{
				SelectedDate = DateTime.Now.Date;
			}

			AddNewVersionCommand = new DelegateCommand(AddNewVersion, () => CanAddNewVersion);
			ChangeVersionStartDateCommand = new DelegateCommand(ChangeVersionStartDate, () => CanChangeVersionStartDate);
		}

		public DelegateCommand AddNewVersionCommand { get; }
		public DelegateCommand ChangeVersionStartDateCommand { get; }

		public IEntityEntryViewModel FuelCardEntryViewModel { get; private set; }

		public bool IsNewCar => Entity.Id == 0;
		public DialogViewModelBase ParentDialog
		{
			get => _parentDialog;
			set
			{
				SetField(ref _parentDialog, value);

				if(_parentDialog == null || FuelCardEntryViewModel != null)
				{
					return;
				}

				FuelCardEntryViewModel = GetFuelCardViewModel();
			}
		}

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					ShowMessageIfSelectedDateNotTodayOrTomorrow();

					OnPropertyChanged(nameof(CanAddNewVersion));
					OnPropertyChanged(nameof(CanChangeVersionStartDate));
				}
			}
		}

		public virtual FuelCard SelectedFuelCard
		{
			get => _selectedFuelCard;
			set
			{
				if(SetField(ref _selectedFuelCard, value))
				{
					OnPropertyChanged(nameof(CanAddNewVersion));
				}
			}
		}

		public virtual FuelCardVersion SelectedVersion
		{
			get => _selectedVersion;
			set
			{
				if(SetField(ref _selectedVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeVersionStartDate));
				}
			}
		}

		public virtual bool CanRead { get; }
		public virtual bool CanCreate { get; }
		public virtual bool CanEdit { get; }
		public virtual bool CanCreateOrUpdate => CanCreate || CanEdit;

		public bool CanAddNewVersion =>
			CanCreateOrUpdate
			&& SelectedDate.HasValue
			&& SelectedFuelCard != null
			&& Entity.FuelCardVersions.All(x => x.Id != 0)
			&& _fuelCardVersionController.IsValidDateForNewCarVersion(SelectedDate.Value, SelectedFuelCard);

		public bool CanChangeVersionStartDate =>
			SelectedDate.HasValue
			&& SelectedVersion != null
			&& (CanEdit || SelectedVersion.Id == 0)
			&& _fuelCardVersionController.IsValidDateForVersionStartDateChange(SelectedVersion, SelectedDate.Value);

		private void AddNewVersion()
		{
			if(SelectedDate == null || SelectedFuelCard == null)
			{
				return;
			}
			_fuelCardVersionController.CreateAndAddVersion(SelectedFuelCard, SelectedDate);

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionStartDate));
		}

		private void ChangeVersionStartDate()
		{
			if(SelectedDate == null || SelectedVersion == null)
			{
				return;
			}
			_fuelCardVersionController.ChangeVersionStartDate(SelectedVersion, SelectedDate.Value);

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionStartDate));
		}

		private IEntityEntryViewModel GetFuelCardViewModel()
		{
			var viewModel = new CommonEEVMBuilderFactory<FuelCardVersionViewModel>(ParentDialog, this, _unitOfWork, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.SelectedFuelCard)
				.UseViewModelDialog<FuelCardViewModel>()
				.UseViewModelJournalAndAutocompleter<FuelCardJournalViewModel, FuelCardJournalFilterViewModel>(filter =>
				{
					filter.IsShowArchived = false;
				})
				.Finish();

			viewModel.IsEditable = CanCreateOrUpdate;
			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelCard)).CanRead;

			return viewModel;
		}

		private void ShowMessageIfSelectedDateNotTodayOrTomorrow()
		{
			if(SelectedDate.HasValue
				&& !_fuelCardVersionController.IsDateTodayOrTomorow(SelectedDate.Value))
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Дата начала действия топливной карты должна быть установлена равной дате сегодня или завтра");
			}
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
