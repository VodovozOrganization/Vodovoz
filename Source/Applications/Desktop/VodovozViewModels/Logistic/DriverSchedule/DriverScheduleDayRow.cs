using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleDayRow : PropertyChangedBase
	{
		private DateTime _date;
		private CarEventType _carEventType;
		private int _morningAddresses;
		private int _morningBottles;
		private int _eveningAddresses;
		private int _eveningBottles;
		private DriverScheduleRow _parentRow;
		private bool _isVirtualCarEventType;
		private bool _isCarEventTypeFromJournal;
		private bool _hasActiveRouteList;

		/// <summary>
		/// Родительская строка
		/// </summary>
		public virtual DriverScheduleRow ParentRow
		{
			get => _parentRow;
			set
			{
				if(SetField(ref _parentRow, value))
				{
					if(_parentRow != null)
					{
						if(!IsPastDay())
						{
							if(_morningAddresses == 0)
							{
								MorningAddresses = _parentRow.MorningAddresses;
							}

							if(_morningBottles == 0)
							{
								MorningBottles = _parentRow.MorningBottles;
							}

							if(_eveningAddresses == 0)
							{
								EveningAddresses = _parentRow.EveningAddresses;
							}

							if(_eveningBottles == 0)
							{
								EveningBottles = _parentRow.EveningBottles;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Вид события ТС
		/// </summary>
		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		/// <summary>
		/// Дата дня
		/// </summary>
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		/// <summary>
		/// Количество адресов утром
		/// </summary>
		public virtual int MorningAddresses
		{
			get => _morningAddresses;
			set => SetField(ref _morningAddresses, value);
		}

		/// <summary>
		/// Количество бутылей утром
		/// </summary>
		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		/// <summary>
		/// Количество адресов вечером
		/// </summary>
		public virtual int EveningAddresses
		{
			get => _eveningAddresses;
			set => SetField(ref _eveningAddresses, value);
		}

		/// <summary>
		/// Количество бутылей вечером
		/// </summary>
		public virtual int EveningBottles
		{
			get => _eveningBottles;
			set => SetField(ref _eveningBottles, value);
		}

		/// <summary>
		/// Имеется активный МЛ
		/// </summary>
		public virtual bool HasActiveRouteList
		{
			get => _hasActiveRouteList;
			set => SetField(ref _hasActiveRouteList, value);
		}

		/// <summary>
		/// Виртуальное событие
		/// </summary>
		public virtual bool IsVirtualCarEventType
		{
			get => _isVirtualCarEventType;
			set => SetField(ref _isVirtualCarEventType, value);
		}

		/// <summary>
		/// Событие ТС из журнала событий ТС
		/// </summary>
		public virtual bool IsCarEventTypeFromJournal
		{
			get => _isCarEventTypeFromJournal;
			set => SetField(ref _isCarEventTypeFromJournal, value);
		}

		private bool IsPastDay()
		{
			if(_date == default || _parentRow?.StartDate == default)
			{
				return false;
			}

			int dayIndex = (int)(_date - _parentRow.StartDate).TotalDays;
			int todayIndex = (int)(DateTime.Today - _parentRow.StartDate).TotalDays;

			return dayIndex < todayIndex;
		}
	}
}

