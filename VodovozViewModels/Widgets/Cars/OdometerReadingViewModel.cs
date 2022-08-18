using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using QS.Commands;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars
{
	public class OdometerReadingsViewModel : EntityWidgetViewModelBase<Car>
	{
		private DateTime? _selectedDate;
		private OdometerReading _selectedOdometerReading;
		private readonly IOdometerReadingsController _odometerReadingController;
		private DelegateCommand _addNewOdometerReadingCommand;
		private DelegateCommand _changeOdometerReadingStartDateCommand;

		public OdometerReadingsViewModel(Car entity, ICommonServices commonServices, IOdometerReadingsController odometerReadingController)
			: base(entity, commonServices)
		{
			_odometerReadingController = odometerReadingController ?? throw new ArgumentNullException(nameof(odometerReadingController));

			CanRead = PermissionResult.CanRead;
			CanCreate = PermissionResult.CanCreate && Entity.Id == 0
				|| commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_odometer_reading");
			CanEdit = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_odometer_reading_date");

			if(IsNewCar)
			{
				SelectedDate = DateTime.Now.Date;
			}
		}

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					OnPropertyChanged(nameof(CanAddNewOdometerReading));
					OnPropertyChanged(nameof(CanChangeOdometerReadingDate));
				}
			}
		}

		public virtual OdometerReading SelectedOdometerReading
		{
			get => _selectedOdometerReading;
			set
			{
				if(SetField(ref _selectedOdometerReading, value))
				{
					OnPropertyChanged(nameof(CanChangeOdometerReadingDate));
				}
			}
		}

		public virtual bool CanRead { get; }
		public virtual bool CanCreate { get; }
		public virtual bool CanEdit { get; }

		public bool IsNewCar => Entity.Id == 0;

		public bool CanAddNewOdometerReading =>
			CanCreate
			&& SelectedDate.HasValue
			&& Entity.OdometerReadings.All(x => x.Id != 0)
			&& _odometerReadingController.IsValidDateForNewOdometerReading(SelectedDate.Value);

		public bool CanChangeOdometerReadingDate =>
			SelectedDate.HasValue
			&& SelectedOdometerReading != null
			&& (CanEdit || SelectedOdometerReading.Id == 0)
			&& _odometerReadingController.IsValidDateForOdometerReadingStartDateChange(SelectedOdometerReading, SelectedDate.Value);


		#region Commands

		public DelegateCommand AddNewOdometerReadingCommand =>
			_addNewOdometerReadingCommand ?? (_addNewOdometerReadingCommand = new DelegateCommand(() =>
				{
					if(SelectedDate == null)
					{
						return;
					}
					_odometerReadingController.CreateAndAddOdometerReading(SelectedDate);

					OnPropertyChanged(nameof(CanAddNewOdometerReading));
					OnPropertyChanged(nameof(CanChangeOdometerReadingDate));
				},
				() => CanAddNewOdometerReading
			));

		public DelegateCommand ChangeOdometerReadingStartDateCommand =>
			_changeOdometerReadingStartDateCommand ?? (_changeOdometerReadingStartDateCommand = new DelegateCommand(() =>
				{
					if(SelectedDate == null || SelectedOdometerReading == null)
					{
						return;
					}
					_odometerReadingController.ChangeOdometerReadingStartDate(SelectedOdometerReading, SelectedDate.Value);

					OnPropertyChanged(nameof(CanAddNewOdometerReading));
					OnPropertyChanged(nameof(CanChangeOdometerReadingDate));
				},
				() => CanChangeOdometerReadingDate
			));

		#endregion
	}
}
