using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleNode : PropertyChangedBase
	{
		private int _driverId;
		private CarTypeOfUse? _carTypeOfUse;
		private CarOwnType? _carOwnType;
		private string _regNumber;
		private string _lastName;
		private string _name;
		private string _patronymic;
		private CarOwnType? _driverCarOwnType;
		private string _driverPhone;
		private District _district;
		private TimeSpan? _arrivalTime;
		private int _morningAddresses;
		private int _morningBottles;
		private int _eveningAddresses;
		private int _eveningBottles;
		private DateTime _lastModifiedDateTime;
		private string _comment;
		private DateTime? _dateFired;
		private DateTime? _dateCalculated;
		private DateTime _startDate;
		private bool _isCarAssigned;
		private int _maxBottles;

		public DriverScheduleNode()
		{
			Days = new DriverScheduleDayNode[7];
			for(int i = 0; i < 7; i++)
			{
				Days[i] = new DriverScheduleDayNode { };
			}
		}

		#region Weekdays

		#region Monday
		public virtual CarEventType MondayCarEventType
		{
			get => GetDayCarEventType(0);
			set => SetDayCarEventType(0, value);
		}

		public virtual int MondayMorningAddress
		{
			get => GetDayMorningAddresses(0);
			set => SetDayMorningAddresses(0, value);
		}

		public virtual int MondayMorningBottles
		{
			get => GetDayMorningBottles(0);
			set => SetDayMorningBottles(0, value);
		}

		public virtual int MondayEveningAddress
		{
			get => GetDayEveningAddresses(0);
			set => SetDayEveningAddresses(0, value);
		}

		public virtual int MondayEveningBottles
		{
			get => GetDayEveningBottles(0);
			set => SetDayEveningBottles(0, value);
		}
		#endregion

		#region Tuesday
		public virtual CarEventType TuesdayCarEventType
		{
			get => GetDayCarEventType(1);
			set => SetDayCarEventType(1, value);
		}

		public virtual int TuesdayMorningAddress
		{
			get => GetDayMorningAddresses(1);
			set => SetDayMorningAddresses(1, value);
		}

		public virtual int TuesdayMorningBottles
		{
			get => GetDayMorningBottles(1);
			set => SetDayMorningBottles(1, value);
		}

		public virtual int TuesdayEveningAddress
		{
			get => GetDayEveningAddresses(1);
			set => SetDayEveningAddresses(1, value);
		}

		public virtual int TuesdayEveningBottles
		{
			get => GetDayEveningBottles(1);
			set => SetDayEveningBottles(1, value);
		}
		#endregion

		#region Wednesday
		public virtual CarEventType WednesdayCarEventType
		{
			get => GetDayCarEventType(2);
			set => SetDayCarEventType(2, value);
		}

		public virtual int WednesdayMorningAddress
		{
			get => GetDayMorningAddresses(2);
			set => SetDayMorningAddresses(2, value);
		}

		public virtual int WednesdayMorningBottles
		{
			get => GetDayMorningBottles(2);
			set => SetDayMorningBottles(2, value);
		}

		public virtual int WednesdayEveningAddress
		{
			get => GetDayEveningAddresses(2);
			set => SetDayEveningAddresses(2, value);
		}

		public virtual int WednesdayEveningBottles
		{
			get => GetDayEveningBottles(2);
			set => SetDayEveningBottles(2, value);
		}
		#endregion

		#region Thursday
		public virtual CarEventType ThursdayCarEventType
		{
			get => GetDayCarEventType(3);
			set => SetDayCarEventType(3, value);
		}

		public virtual int ThursdayMorningAddress
		{
			get => GetDayMorningAddresses(3);
			set => SetDayMorningAddresses(3, value);
		}

		public virtual int ThursdayMorningBottles
		{
			get => GetDayMorningBottles(3);
			set => SetDayMorningBottles(3, value);
		}

		public virtual int ThursdayEveningAddress
		{
			get => GetDayEveningAddresses(3);
			set => SetDayEveningAddresses(3, value);
		}

		public virtual int ThursdayEveningBottles
		{
			get => GetDayEveningBottles(3);
			set => SetDayEveningBottles(3, value);
		}
		#endregion

		#region Friday
		public virtual CarEventType FridayCarEventType
		{
			get => GetDayCarEventType(4);
			set => SetDayCarEventType(4, value);
		}

		public virtual int FridayMorningAddress
		{
			get => GetDayMorningAddresses(4);
			set => SetDayMorningAddresses(4, value);
		}

		public virtual int FridayMorningBottles
		{
			get => GetDayMorningBottles(4);
			set => SetDayMorningBottles(4, value);
		}

		public virtual int FridayEveningAddress
		{
			get => GetDayEveningAddresses(4);
			set => SetDayEveningAddresses(4, value);
		}

		public virtual int FridayEveningBottles
		{
			get => GetDayEveningBottles(4);
			set => SetDayEveningBottles(4, value);
		}
		#endregion

		#region Saturday
		public virtual CarEventType SaturdayCarEventType
		{
			get => GetDayCarEventType(5);
			set => SetDayCarEventType(5, value);
		}

		public virtual int SaturdayMorningAddress
		{
			get => GetDayMorningAddresses(5);
			set => SetDayMorningAddresses(5, value);
		}

		public virtual int SaturdayMorningBottles
		{
			get => GetDayMorningBottles(5);
			set => SetDayMorningBottles(5, value);
		}

		public virtual int SaturdayEveningAddress
		{
			get => GetDayEveningAddresses(5);
			set => SetDayEveningAddresses(5, value);
		}

		public virtual int SaturdayEveningBottles
		{
			get => GetDayEveningBottles(5);
			set => SetDayEveningBottles(5, value);
		}
		#endregion

		#region Sunday
		public virtual CarEventType SundayCarEventType
		{
			get => GetDayCarEventType(6);
			set => SetDayCarEventType(6, value);
		}

		public virtual int SundayMorningAddress
		{
			get => GetDayMorningAddresses(6);
			set => SetDayMorningAddresses(6, value);
		}

		public virtual int SundayMorningBottles
		{
			get => GetDayMorningBottles(6);
			set => SetDayMorningBottles(6, value);
		}

		public virtual int SundayEveningAddress
		{
			get => GetDayEveningAddresses(6);
			set => SetDayEveningAddresses(6, value);
		}

		public virtual int SundayEveningBottles
		{
			get => GetDayEveningBottles(6);
			set => SetDayEveningBottles(6, value);
		}
		#endregion

		#endregion

		#region Helper Methods

		public CarEventType GetDayCarEventType(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.CarEventType : null;
		}

		private bool CanSetDayValue(int dayIndex, int value)
		{
			var dayEventType = GetDayCarEventType(dayIndex);
			if(dayEventType != null && dayEventType.Id > 0)
			{
				return false;
			}

			return _isCarAssigned || value <= 0;
		}

		private int GetValidatedBottleValue(int dayIndex, int value)
		{
			if(!CanSetDayValue(dayIndex, value))
			{
				return 0;
			}

			return ClampBottleValue(value);
		}

		private int ClampBottleValue(int value)
		{
			return _maxBottles > 0 && value > _maxBottles ? _maxBottles : value;
		}

		public void SetDayCarEventType(int dayIndex, CarEventType value)
		{
			if(!IsValidDayIndex(dayIndex))
			{
				return;
			}

			Days[dayIndex].CarEventType = value;

			if(value != null && value.Id > 0)
			{
				Days[dayIndex].MorningAddresses = 0;
				Days[dayIndex].MorningBottles = 0;
				Days[dayIndex].EveningAddresses = 0;
				Days[dayIndex].EveningBottles = 0;
			}
		}

		public int GetDayMorningAddresses(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.MorningAddresses ?? 0 : 0;
		}

		public void SetDayMorningAddresses(int dayIndex, int value)
		{
			if(!IsValidDayIndex(dayIndex))
			{
				return;
			}

			Days[dayIndex].MorningAddresses = CanSetDayValue(dayIndex, value) ? value : 0;
		}

		public int GetDayMorningBottles(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.MorningBottles ?? 0 : 0;
		}

		public void SetDayMorningBottles(int dayIndex, int value)
		{
			if(!IsValidDayIndex(dayIndex))
			{
				return;
			}

			Days[dayIndex].MorningBottles = GetValidatedBottleValue(dayIndex, value);
		}

		public int GetDayEveningAddresses(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.EveningAddresses ?? 0 : 0;
		}

		public void SetDayEveningAddresses(int dayIndex, int value)
		{
			if(!IsValidDayIndex(dayIndex))
			{
				return;
			}

			Days[dayIndex].EveningAddresses = CanSetDayValue(dayIndex, value) ? value : 0;
		}

		public int GetDayEveningBottles(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.EveningBottles ?? 0 : 0;
		}

		public void SetDayEveningBottles(int dayIndex, int value)
		{
			if(!IsValidDayIndex(dayIndex))
			{
				return;
			}

			Days[dayIndex].EveningBottles = GetValidatedBottleValue(dayIndex, value);
		}

		public bool IsValidDayIndex(int dayIndex)
		{
			return Days != null && dayIndex >= 0 && dayIndex < Days.Length;
		}

		#endregion

		#region Properties

		[Display(Name = "Дни расписания")]
		public DriverScheduleDayNode[] Days;

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual int DriverId
		{
			get => _driverId;
			set => SetField(ref _driverId, value);
		}

		public virtual CarTypeOfUse? CarTypeOfUse
		{
			get => _carTypeOfUse;
			set => SetField(ref _carTypeOfUse, value);
		}

		public virtual CarOwnType? CarOwnType
		{
			get => _carOwnType;
			set => SetField(ref _carOwnType, value);
		}

		public virtual string RegNumber
		{
			get => _regNumber;
			set => SetField(ref _regNumber, value);
		}

		public virtual string LastName
		{
			get => _lastName;
			set => SetField(ref _lastName, value);
		}

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value);
		}

		public virtual CarOwnType? DriverCarOwnType
		{
			get => _driverCarOwnType;
			set => SetField(ref _driverCarOwnType, value);
		}

		public virtual string DriverPhone
		{
			get => _driverPhone;
			set => SetField(ref _driverPhone, value);
		}

		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		/// <summary>
		/// Время приезда
		/// </summary>
		public virtual TimeSpan? ArrivalTime
		{
			get => _arrivalTime;
			set => SetField(ref _arrivalTime, value);
		}

		public virtual int MorningAddresses
		{
			get => _morningAddresses;
			set
			{
				if(SetField(ref _morningAddresses, value))
				{
					UpdateDayValuesFromPotential();
				}
			}
		}

		public virtual int MorningBottles
		{
			get => _morningBottles;
			set
			{
				int finalValue = ClampBottleValue(value);
				if(SetField(ref _morningBottles, finalValue))
				{
					UpdateDayValuesFromPotential();
				}
			}
		}

		public virtual int EveningAddresses
		{
			get => _eveningAddresses;
			set
			{
				if(SetField(ref _eveningAddresses, value))
				{
					UpdateDayValuesFromPotential();
				}
			}
		}

		public virtual int EveningBottles
		{
			get => _eveningBottles;
			set
			{
				int finalValue = ClampBottleValue(value);
				if(SetField(ref _eveningBottles, finalValue))
				{
					UpdateDayValuesFromPotential();
				}
			}
		}

		public virtual DateTime LastModifiedDateTime
		{
			get => _lastModifiedDateTime;
			set => SetField(ref _lastModifiedDateTime, value);
		}

		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public virtual DateTime? DateFired
		{
			get => _dateFired;
			set => SetField(ref _dateFired, value);
		}

		public virtual DateTime? DateCalculated
		{
			get => _dateCalculated;
			set => SetField(ref _dateCalculated, value);
		}

		/// <summary>
		/// Привязан ли водитель к авто
		/// </summary>
		public virtual bool IsCarAssigned
		{
			get => _isCarAssigned;
			set => SetField(ref _isCarAssigned, value);
		}

		public virtual int MaxBottles
		{
			get => _maxBottles;
			set => SetField(ref _maxBottles, value);
		}

		public string LastModifiedDateTimeString =>
			LastModifiedDateTime != default
				? LastModifiedDateTime.ToString("g")
				: "Нет";

		public string CarTypeOfUseString => CarTypeOfUse.HasValue
			? Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarTypeOfUse.Value)
			: "";

		public string CarOwnTypeString => CarOwnType.HasValue
			? Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarOwnType.Value)
			: "";

		public string DriverCarOwnTypeString => DriverCarOwnType.HasValue
			? Gamma.Utilities.AttributeUtil.GetEnumTitle(DriverCarOwnType.Value)
			: "";

		public string DistrictString => District?.DistrictName ?? "";

		public string DriverFullName => string.Join(" ",
			new[] { LastName, Name, Patronymic }
				.Where(x => !string.IsNullOrWhiteSpace(x)));

		#endregion

		public void InitializeEmptyCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = -1, ShortName = "Нет", Name = "Нет" };

			foreach (var day in Days)
			{
				if(day?.IsVirtualCarEventType == true || (day?.CarEventType != null && day.CarEventType.Id > 0))
				{
					continue;
				}

				if(day != null)
				{
					day.CarEventType = noneEventType;
				}
			}
		}

		public virtual DateTime? GetDismissalDate()
		{
			if(_dateFired.HasValue && _dateCalculated.HasValue)
			{
				return _dateFired.Value < _dateCalculated.Value ? _dateFired : _dateCalculated;
			}

			return _dateFired ?? _dateCalculated;
		}

		/// <summary>
		/// Обновляет дневные значения на основе потенциалов начиная с текущего дня
		/// </summary>
		private void UpdateDayValuesFromPotential()
		{
			if(StartDate == default)
			{
				return;
			}

			int todayIndex = (int)(DateTime.Today - StartDate).TotalDays;

			if(todayIndex < 0 || todayIndex >= 7)
			{
				return;
			}

			for(int i = todayIndex; i < 7; i++)
			{
				if(Days[i] != null)
				{
					if(Days[i].CarEventType == null || Days[i].CarEventType.Id == 0)
					{
						Days[i].MorningAddresses = _morningAddresses;
						Days[i].MorningBottles = _morningBottles;
						Days[i].EveningAddresses = _eveningAddresses;
						Days[i].EveningBottles = _eveningBottles;
					}
				}
			}
		}
	}
}
