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
		Func<DateTime?, CarVersion, bool> _canChangeVersionDateFunc;

		public event EventHandler<AddNewVersionEventArgs> AddNewVersionClicked;
		public event EventHandler<EditStartDateEventArgs> EditStartDateClicked;
		public event EventHandler<EditCarOwnerEventArgs> EditCarOwnerClicked;

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
					OnPropertyChanged(nameof(CanAddNewVersion));
					OnPropertyChanged(nameof(CanChangeVersionDate));
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
					OnPropertyChanged(nameof(CanChangeVersionDate));
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

		public bool CanChangeVersionDate =>
			_canChangeVersionDateFunc?.Invoke(SelectedDate, SelectedCarVersion) ?? false;

		public void Initialize(
			Func<DateTime?, bool> canAddNewVersionFunc,
			Func<DateTime?, CarVersion, bool> canChangeVersionDateFunc,
			bool isNewCar,
			bool isWidgetVisible)
		{
			if(!(_canAddNewVersionFunc is null)
				|| !(_canChangeVersionDateFunc is null))
			{
				throw new InvalidOperationException($"Инициализация виджета уже была выполнена");
			}

			_canAddNewVersionFunc = canAddNewVersionFunc ?? throw new ArgumentNullException(nameof(canAddNewVersionFunc));
			_canChangeVersionDateFunc = canChangeVersionDateFunc ?? throw new ArgumentNullException(nameof(canChangeVersionDateFunc));
			IsNewCar = isNewCar;
			IsWidgetVisible = isWidgetVisible;

			if(IsNewCar)
			{
				SelectedDate = DateTime.Today;
			}
		}

		public void AddNewCarVersion()
		{
			if(SelectedDate == null)
			{
				return;
			}

			AddNewVersionClicked?.Invoke(this, new AddNewVersionEventArgs(SelectedDate.Value));

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}

		public void ChangeVersionStartDate()
		{
			if(SelectedDate == null || SelectedCarVersion == null)
			{
				return;
			}

			EditStartDateClicked?.Invoke(this, new EditStartDateEventArgs(SelectedCarVersion, SelectedDate.Value));

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}
	}

	public class AddNewVersionEventArgs : EventArgs
	{
		public AddNewVersionEventArgs(DateTime startDateTime)
		{
			StartDateTime = startDateTime;
		}

		public DateTime StartDateTime { get; }
	}

	public class EditStartDateEventArgs : EventArgs
	{
		public EditStartDateEventArgs(CarVersion carVersion, DateTime startDate)
		{
			CarVersion = carVersion;
			StartDate = startDate;
		}

		public CarVersion CarVersion { get; }
		public DateTime StartDate { get; }
	}

	public class EditCarOwnerEventArgs : EventArgs
	{
		public EditCarOwnerEventArgs(CarVersion carVersion)
		{
			CarVersion = carVersion;
		}

		public CarVersion CarVersion { get; }
	}
}
