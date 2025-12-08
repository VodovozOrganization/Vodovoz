using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class CarVersionEditingViewModel : WidgetViewModelBase
	{
		private CarVersion _carVersion;
		private CarOwnType? _selectedCarOwnType;
		private Organization _selectedCarOwner;
		private CarVersion _lastCarVersion;
		private bool _isWidgetVisible;
		private DialogViewModelBase _parentDialog;
		IList<CarOwnType> _availableCarOwnTypes;
		private readonly ICommonServices _commonServices;

		public CarVersionEditingViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(CarVersionEditingViewModel));
			SetPermissions();

			SaveCarVersionCommand = new DelegateCommand(SaveCarVersion, () => CanSaveCarVersion);
			CancelEditingCommand = new DelegateCommand(CancelEditing);
		}

		public DelegateCommand SaveCarVersionCommand { get; }
		public DelegateCommand CancelEditingCommand { get; }

		public event EventHandler SaveCarVersionClicked;
		public event EventHandler CancelEditingClicked;

		public IUnitOfWork UnitOfWork { get; }
		public INavigationManager NavigationManager { get; }
		public ILifetimeScope LifetimeScope { get; }

		[PropertyChangedAlso(nameof(CanSelectCarOwner), nameof(CanSaveCarVersion))]
		public CarOwnType? SelectedCarOwnType
		{
			get => _selectedCarOwnType;
			set
			{
				SetField(ref _selectedCarOwnType, value);

				if(_selectedCarOwnType == CarOwnType.Driver)
				{
					SelectedCarOwner = null;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanSaveCarVersion))]
		public Organization SelectedCarOwner
		{
			get => _selectedCarOwner;
			set => SetField(ref _selectedCarOwner, value);
		}

		public CarVersion LastCarVersion
		{
			get => _lastCarVersion;
			set => SetField(ref _lastCarVersion, value);
		}

		public bool IsWidgetVisible
		{
			get => _isWidgetVisible;
			set => SetField(ref _isWidgetVisible, value);
		}

		public DialogViewModelBase ParentDialog
		{
			get => _parentDialog;
			set
			{
				if(!(_parentDialog is null))
				{
					return;
				}

				SetField(ref _parentDialog, value);
			}
		}

		public IList<CarOwnType> AvailableCarOwnTypes
		{
			get => _availableCarOwnTypes;
			set => SetField(ref _availableCarOwnTypes, value);
		}

		public bool CanEditCarOwnType =>
			AvailableCarOwnTypes?.Count > 0
			&& !(_carVersion is null)
			&& _carVersion.Id == 0;

		public bool CanSelectCarOwner =>
			SelectedCarOwnType.HasValue
			&& SelectedCarOwnType != CarOwnType.Driver;

		public bool CanSaveCarVersion => !CanSelectCarOwner || !(SelectedCarOwner is null);

		public bool IsVersionNewAndTypeWithOwnerOrganizationEqualsLastVersion =>
			_carVersion.Id == 0
			&& SelectedCarOwner?.Id == LastCarVersion?.CarOwnerOrganization?.Id
			&& SelectedCarOwnType == LastCarVersion?.CarOwnType;
		
		private bool CanChangeCompositionCompanyTransportPark { get; set; }
		
		private void SetPermissions()
		{
			CanChangeCompositionCompanyTransportPark =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(CarPermissions.CanChangeCompositionCompanyTransportPark);
		}

		private void SaveCarVersion()
		{
			if(!CanSaveCarVersion)
			{
				return;
			}

			if(!CanChangeCompositionCompanyTransportPark)
			{
				const string message = "Невозможно изменить принадлежность авто. У Вас нет права менять состав автопарка компании";
				
				if(((LastCarVersion is null || LastCarVersion.CarOwnType == CarOwnType.Driver)
					&& (SelectedCarOwnType == CarOwnType.Raskat || SelectedCarOwnType == CarOwnType.Company))
					|| LastCarVersion != null && (LastCarVersion.IsCompanyCar || LastCarVersion.IsRaskat))
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
					return;
				}
			}

			if(IsVersionNewAndTypeWithOwnerOrganizationEqualsLastVersion)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error, 
					"В новой версии должен отличаться либо собственник авто, либо принадлежность авто");

				return;
			}

			_carVersion.CarOwnType = SelectedCarOwnType.Value;
			_carVersion.CarOwnerOrganization = SelectedCarOwner;

			ClearWidgetProperties();
			SaveCarVersionClicked?.Invoke(this, EventArgs.Empty);
		}

		private void CancelEditing()
		{
			ClearWidgetProperties();
			CancelEditingClicked?.Invoke(this, EventArgs.Empty);
		}

		public void SetWidgetProperties(CarVersion carVersion, IList<CarOwnType> availableCarOwnTypes, CarVersion lastCarVersion)
		{
			_carVersion = carVersion ?? throw new ArgumentNullException(nameof(carVersion));
			AvailableCarOwnTypes = availableCarOwnTypes ?? throw new ArgumentNullException(nameof(availableCarOwnTypes));

			if(lastCarVersion != null)
			{
				LastCarVersion = lastCarVersion;
			}

			SelectedCarOwnType = _carVersion.CarOwnType;
			SelectedCarOwner = _carVersion.CarOwnerOrganization;

			IsWidgetVisible = true;

			OnPropertyChanged(nameof(CanEditCarOwnType));
			OnPropertyChanged(nameof(CanSelectCarOwner));
			OnPropertyChanged(nameof(CanSaveCarVersion));
		}

		private void ClearWidgetProperties()
		{
			SelectedCarOwnType = null;
			SelectedCarOwner = null;
			AvailableCarOwnTypes = null;
			_carVersion = null;
			IsWidgetVisible = false;
		}
	}
}
