using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleDayNode : PropertyChangedBase
	{
		private DateTime _date;
		private CarEventType _carEventType;
		private int _morningAddresses;
		private int _morningBottles;
		private int _eveningAddresses;
		private int _eveningBottles;
		private DriverScheduleNode _parentNode;
		private bool _isVirtualCarEventType;
		private bool _isCarEventTypeFromJournal;
		private bool _hasActiveRouteList;

		public virtual DriverScheduleNode ParentNode
		{
			get => _parentNode;
			set
			{
				if(SetField(ref _parentNode, value))
				{
					if(_parentNode != null)
					{
						if(!IsPastDay())
						{
							if(_morningAddresses == 0)
							{
								MorningAddresses = _parentNode.MorningAddresses;
							}

							if(_morningBottles == 0)
							{
								MorningBottles = _parentNode.MorningBottles;
							}

							if(_eveningAddresses == 0)
							{
								EveningAddresses = _parentNode.EveningAddresses;
							}

							if(_eveningBottles == 0)
							{
								EveningBottles = _parentNode.EveningBottles;
							}
						}
					}
				}
			}
		}

		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		public virtual int MorningAddresses
		{
			get => _morningAddresses;
			set => SetField(ref _morningAddresses, value);
		}

		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		public virtual int EveningAddresses
		{
			get => _eveningAddresses;
			set => SetField(ref _eveningAddresses, value);
		}

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
			if(_date == default || _parentNode?.StartDate == default)
			{
				return false;
			}

			int dayIndex = (int)(_date - _parentNode.StartDate).TotalDays;
			int todayIndex = (int)(DateTime.Today - _parentNode.StartDate).TotalDays;

			return dayIndex < todayIndex;
		}
	}
}

