using Autofac;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class CarVersionEditingViewModel : WidgetViewModelBase
	{
		private CarVersion _carVersion;
		private CarOwnType? _selectedCarOwnType;
		private Organization _selectedCarOwner;
		private bool _isWidgetVisible;
		private DialogViewModelBase _parentDialog;
		IList<CarOwnType> _availableCarOwnTypes;

		public CarVersionEditingViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(CarVersionEditingViewModel));

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

		private void SaveCarVersion()
		{
			if(!CanSaveCarVersion)
			{
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

		public void SetWidgetProperties(CarVersion carVersion, IList<CarOwnType> availableCarOwnTypes)
		{
			_carVersion = carVersion ?? throw new ArgumentNullException(nameof(carVersion));
			AvailableCarOwnTypes = availableCarOwnTypes ?? throw new ArgumentNullException(nameof(availableCarOwnTypes));

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
