using System;
using System.ComponentModel;

namespace DateTimeHelpers
{
	public class DateTimePeriod : INotifyPropertyChanged
	{
		private DateTime? _startDateTime;
		private DateTime? _endDateTime;

		public DateTime? StartDateTime
		{
			get => _startDateTime;
			set
			{
				if(_startDateTime != value)
				{
					_startDateTime = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartDateTime)));
				}
			}
		}

		public DateTime? EndDateTime
		{
			get => _endDateTime;
			set
			{
				if(_endDateTime != value)
				{
					_endDateTime = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDateTime)));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
