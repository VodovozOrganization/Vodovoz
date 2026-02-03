using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DayScheduleNode : PropertyChangedBase
	{
		private DateTime? _date;
		private CarEventType _carEventType;
		private int? _morningAddress;
		private int? _morningBottles;
		private int? _eveningAddress;
		private int? _eveningBottles;
		private DriverScheduleDatasetNode _parentNode;

		public virtual DriverScheduleDatasetNode ParentNode
		{
			get => _parentNode;
			set => SetField(ref _parentNode, value);
		}

		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		public virtual DateTime? Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		public virtual int? MorningAddress
		{
			get => _morningAddress;
			set => SetField(ref _morningAddress, value);
		}

		public virtual int? MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		public virtual int? EveningAddress
		{
			get => _eveningAddress;
			set => SetField(ref _eveningAddress, value);
		}

		public virtual int? EveningBottles
		{
			get => _eveningBottles;
			set => SetField(ref _eveningBottles, value);
		}

		public string DateString
		{
			get
			{
				if(_date == default || !_date.HasValue)
				{
					return "";
				}

				string dayOfWeek = GetDayOfWeekShort(_date.Value.Date.DayOfWeek);

				return $"{dayOfWeek},{_date:dd.MM.yyyy}";
			}
		}

		private string GetDayOfWeekShort(DayOfWeek dayOfWeek)
		{
			switch(dayOfWeek)
			{
				case DayOfWeek.Monday: return "Пн";
				case DayOfWeek.Tuesday: return "Вт";
				case DayOfWeek.Wednesday: return "Ср";
				case DayOfWeek.Thursday: return "Чт";
				case DayOfWeek.Friday: return "Пт";
				case DayOfWeek.Saturday: return "Сб";
				case DayOfWeek.Sunday: return "Вс";
				default: return "";
			}
		}
	}
}

