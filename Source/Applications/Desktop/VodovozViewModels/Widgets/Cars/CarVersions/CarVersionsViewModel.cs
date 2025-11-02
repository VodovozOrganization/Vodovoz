using QS.Commands;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class CarVersionsViewModel : WidgetViewModelBase
	{
		private IList<CarVersion> _carVersions;
		private DateTime? _selectedDate;
		private CarVersion _selectedCarVersion;
		private bool _isNewCar;
		private bool _isWidgetVisible;
		Func<DateTime?, bool> _canAddNewVersionFunc;
		Func<DateTime?, CarVersion, bool> _canChangeVersionStartDateFunc;
		Func<CarVersion, bool> _canEditCarOwnerFunc;

		public CarVersionsViewModel()
		{
			AddNewVersionCommand = new DelegateCommand(AddNewCarVersion, () => CanAddNewVersion);
			ChangeStartDateCommand = new DelegateCommand(ChangeVersionStartDate, () => CanChangeVersionStartDate);
			EditCarOwnerCommand = new DelegateCommand(EditCarOwner, () => CanEditCarOwner);
		}

		public event EventHandler<AddNewVersionEventArgs> AddNewVersionClicked;
		public event EventHandler<ChangeStartDateEventArgs> ChangeStartDateClicked;
		public event EventHandler<EditCarOwnerEventArgs> EditCarOwnerClicked;

		public DelegateCommand AddNewVersionCommand;
		public DelegateCommand ChangeStartDateCommand;
		public DelegateCommand EditCarOwnerCommand;

		public IList<CarVersion> CarVersions
		{
			get => _carVersions;
			set => SetField(ref _carVersions, value);
		}

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					UpdateAccessibilityProperties();
				}
			}
		}

		public virtual CarVersion SelectedCarVersion
		{
			get => _selectedCarVersion;
			set
			{
				if(SetField(ref _selectedCarVersion, value))
				{
					UpdateAccessibilityProperties();
				}
			}
		}

		public bool IsNewCar
		{
			get => _isNewCar;
			private set => SetField(ref _isNewCar, value);
		}

		public bool IsWidgetVisible
		{
			get => _isWidgetVisible;
			private set => SetField(ref _isWidgetVisible, value);
		}

		public bool CanAddNewVersion =>
			_canAddNewVersionFunc?.Invoke(SelectedDate) ?? false;

		public bool CanChangeVersionStartDate =>
			_canChangeVersionStartDateFunc?.Invoke(SelectedDate, SelectedCarVersion) ?? false;

		public bool CanEditCarOwner =>
			_canEditCarOwnerFunc?.Invoke(SelectedCarVersion) ?? false;

		public void Initialize(
			Func<DateTime?, bool> canAddNewVersionFunc,
			Func<DateTime?, CarVersion, bool> canChangeVersionDateFunc,
			Func<CarVersion, bool> canEditCarOwnerFunc,
			bool isNewCar,
			bool isWidgetVisible)
		{
			if(!(_canAddNewVersionFunc is null)
				|| !(_canChangeVersionStartDateFunc is null)
				|| !(_canEditCarOwnerFunc is null))
			{
				throw new InvalidOperationException($"Инициализация виджета уже была выполнена");
			}

			_canAddNewVersionFunc = canAddNewVersionFunc ?? throw new ArgumentNullException(nameof(canAddNewVersionFunc));
			_canChangeVersionStartDateFunc = canChangeVersionDateFunc ?? throw new ArgumentNullException(nameof(canChangeVersionDateFunc));
			_canEditCarOwnerFunc = canEditCarOwnerFunc ?? throw new ArgumentNullException(nameof(canEditCarOwnerFunc));

			IsNewCar = isNewCar;
			IsWidgetVisible = isWidgetVisible;

			if(IsNewCar)
			{
				SelectedDate = DateTime.Today;
			}
		}

		public void AddNewCarVersion()
		{
			if(SelectedDate is null || !CanAddNewVersion)
			{
				return;
			}

			AddNewVersionClicked?.Invoke(this, new AddNewVersionEventArgs(SelectedDate.Value));

			UpdateAccessibilityProperties();
		}

		public void ChangeVersionStartDate()
		{
			if(!SelectedDate.HasValue || SelectedCarVersion is null || !CanChangeVersionStartDate)
			{
				return;
			}

			ChangeStartDateClicked?.Invoke(this, new ChangeStartDateEventArgs(SelectedCarVersion, SelectedDate.Value));

			UpdateAccessibilityProperties();
		}

		public void EditCarOwner()
		{
			if(SelectedCarVersion is null || !CanEditCarOwner)
			{
				return;
			}

			EditCarOwnerClicked?.Invoke(this, new EditCarOwnerEventArgs(SelectedCarVersion));

			UpdateAccessibilityProperties();
		}

		public void UpdateAccessibilityProperties()
		{
			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionStartDate));
			OnPropertyChanged(nameof(CanEditCarOwner));
		}
	}
}
