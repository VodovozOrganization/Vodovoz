using System;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class DriversWarehousesEventsJournalFilterViewModel : FilterViewModelBase<DriversWarehousesEventsJournalFilterViewModel>
	{
		private int? _eventId;
		private string _eventName;
		private decimal? _eventLatitude;
		private decimal? _eventLongitude;
		private DriverWarehouseEventType? _selectedEventType;

		public DriversWarehousesEventsJournalFilterViewModel(
			Action<DriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}
		
		public IEntityEntryViewModel DriverWarehouseEventNameViewModel { get; private set; }

		public int? EventId
		{
			get => _eventId;
			set => SetField(ref _eventId, value);
		}

		public string EventName
		{
			get => _eventName;
			set => SetField(ref _eventName, value);
		}

		public DriverWarehouseEventType? SelectedEventType
		{
			get => _selectedEventType;
			set => UpdateFilterField(ref _selectedEventType, value);
		}

		public decimal? EventLatitude
		{
			get => _eventLatitude;
			set => SetField(ref _eventLatitude, value);
		}

		public decimal? EventLongitude
		{
			get => _eventLongitude;
			set => SetField(ref _eventLongitude, value);
		}
	}
}
