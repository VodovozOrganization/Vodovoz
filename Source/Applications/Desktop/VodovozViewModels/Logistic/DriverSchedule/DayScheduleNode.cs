using QS.DomainModel.Entity;
using System;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DayScheduleNode : PropertyChangedBase
	{
		private DateTime _date;
		private string _morningAddress;
		private int _morningBottles;
		private string _eveningAddress;
		private int _eveningBottles;

		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		public virtual string MorningAddress
		{
			get => _morningAddress;
			set => SetField(ref _morningAddress, value);
		}

		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		public virtual string EveningAddress
		{
			get => _eveningAddress;
			set => SetField(ref _eveningAddress, value);
		}

		public virtual int EveningBottles
		{
			get => _eveningBottles;
			set => SetField(ref _eveningBottles, value);
		}

		public string DateString
		{
			get
			{
				if(_date == default)
				{
					return "";
				}

				string dayOfWeek = GetDayOfWeekShort(_date.DayOfWeek);

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

