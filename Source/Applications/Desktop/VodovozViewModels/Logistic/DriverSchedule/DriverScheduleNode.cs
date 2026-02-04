using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleNode : PropertyChangedBase
	{
		private int _driverId;
		private CarTypeOfUse _carTypeOfUse;
		private CarOwnType _carOwnType;
		private string _regNumber;
		private string _lastName;
		private string _name;
		private string _patronymic;
		private CarOwnType _driverCarOwnType;
		private string _driverPhone;
		private District _district;
		private DeliverySchedule _deliverySchedule;
		private int _morningAddress;
		private int _morningBottles;
		private int _eveningAddress;
		private int _eveningBottles;
		private DateTime _lastModifiedDateTime;
		private string _comment;

		public DriverScheduleNode()
		{
			Days = new DriverScheduleDayNode[7];
			for(int i = 0; i < 7; i++)
			{
				Days[i] = new DriverScheduleDayNode { };
			}
		}

		[Display(Name = "Дни расписания")]
		public DriverScheduleDayNode[] Days;

		#region Weekdays

		#region Monday
		public virtual CarEventType MondayCarEventType
		{
			get => GetDayCarEventType(0);
			set => SetDayCarEventType(0, value);
		}

		public virtual int MondayMorningAddress
		{
			get => GetDayMorningAddress(0);
			set => SetDayMorningAddress(0, value);
		}

		public virtual int MondayMorningBottles
		{
			get => GetDayMorningBottles(0);
			set => SetDayMorningBottles(0, value);
		}

		public virtual int MondayEveningAddress
		{
			get => GetDayEveningAddress(0);
			set => SetDayEveningAddress(0, value);
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
			get => GetDayMorningAddress(1);
			set => SetDayMorningAddress(1, value);
		}

		public virtual int TuesdayMorningBottles
		{
			get => GetDayMorningBottles(1);
			set => SetDayMorningBottles(1, value);
		}

		public virtual int TuesdayEveningAddress
		{
			get => GetDayEveningAddress(1);
			set => SetDayEveningAddress(1, value);
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
			get => GetDayMorningAddress(2);
			set => SetDayMorningAddress(2, value);
		}

		public virtual int WednesdayMorningBottles
		{
			get => GetDayMorningBottles(2);
			set => SetDayMorningBottles(2, value);
		}

		public virtual int WednesdayEveningAddress
		{
			get => GetDayEveningAddress(2);
			set => SetDayEveningAddress(2, value);
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
			get => GetDayMorningAddress(3);
			set => SetDayMorningAddress(3, value);
		}

		public virtual int ThursdayMorningBottles
		{
			get => GetDayMorningBottles(3);
			set => SetDayMorningBottles(3, value);
		}

		public virtual int ThursdayEveningAddress
		{
			get => GetDayEveningAddress(3);
			set => SetDayEveningAddress(3, value);
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
			get => GetDayMorningAddress(4);
			set => SetDayMorningAddress(4, value);
		}

		public virtual int FridayMorningBottles
		{
			get => GetDayMorningBottles(4);
			set => SetDayMorningBottles(4, value);
		}

		public virtual int FridayEveningAddress
		{
			get => GetDayEveningAddress(4);
			set => SetDayEveningAddress(4, value);
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
			get => GetDayMorningAddress(5);
			set => SetDayMorningAddress(5, value);
		}

		public virtual int SaturdayMorningBottles
		{
			get => GetDayMorningBottles(5);
			set => SetDayMorningBottles(5, value);
		}

		public virtual int SaturdayEveningAddress
		{
			get => GetDayEveningAddress(5);
			set => SetDayEveningAddress(5, value);
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
			get => GetDayMorningAddress(6);
			set => SetDayMorningAddress(6, value);
		}

		public virtual int SundayMorningBottles
		{
			get => GetDayMorningBottles(6);
			set => SetDayMorningBottles(6, value);
		}

		public virtual int SundayEveningAddress
		{
			get => GetDayEveningAddress(6);
			set => SetDayEveningAddress(6, value);
		}

		public virtual int SundayEveningBottles
		{
			get => GetDayEveningBottles(6);
			set => SetDayEveningBottles(6, value);
		}
		#endregion

		#endregion

		#region Helper Methods

		private CarEventType GetDayCarEventType(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.CarEventType : null;
		}

		private void SetDayCarEventType(int dayIndex, CarEventType value)
		{
			if(IsValidDayIndex(dayIndex))
				Days[dayIndex].CarEventType = value;
		}

		private int GetDayMorningAddress(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.MorningAddresses ?? 0 : 0;
		}

		private void SetDayMorningAddress(int dayIndex, int value)
		{
			if(IsValidDayIndex(dayIndex))
				Days[dayIndex].MorningAddresses = value;
		}

		private int GetDayMorningBottles(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.MorningBottles ?? 0 : 0;
		}

		private void SetDayMorningBottles(int dayIndex, int value)
		{
			if(IsValidDayIndex(dayIndex))
				Days[dayIndex].MorningBottles = value;
		}

		private int GetDayEveningAddress(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.EveningAddresses ?? 0 : 0;
		}

		private void SetDayEveningAddress(int dayIndex, int value)
		{
			if(IsValidDayIndex(dayIndex))
				Days[dayIndex].EveningAddresses = value;
		}

		private int GetDayEveningBottles(int dayIndex)
		{
			return IsValidDayIndex(dayIndex) ? Days[dayIndex]?.EveningBottles ?? 0 : 0;
		}

		private void SetDayEveningBottles(int dayIndex, int value)
		{
			if(IsValidDayIndex(dayIndex))
				Days[dayIndex].EveningBottles = value;
		}

		private bool IsValidDayIndex(int dayIndex)
		{
			return Days != null && dayIndex >= 0 && dayIndex < Days.Length;
		}

		#endregion

		public virtual int DriverId
		{
			get => _driverId;
			set => SetField(ref _driverId, value);
		}

		public virtual CarTypeOfUse CarTypeOfUse
		{
			get => _carTypeOfUse;
			set => SetField(ref _carTypeOfUse, value);
		}

		public virtual CarOwnType CarOwnType
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

		public virtual CarOwnType DriverCarOwnType
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

		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}

		public virtual int MorningAddresses
		{
			get => _morningAddress;
			set => SetField(ref _morningAddress, value);
		}

		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		public virtual int EveningAddresses
		{
			get => _eveningAddress;
			set => SetField(ref _eveningAddress, value);
		}

		public virtual int EveningBottles
		{
			get => _eveningBottles;
			set => SetField(ref _eveningBottles, value);
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

		public string LastModifiedDateTimeString =>
			LastModifiedDateTime != default
				? LastModifiedDateTime.ToString("g")
				: "Нет";

		public string CarTypeOfUseString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarTypeOfUse);

		public string CarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarOwnType);

		public string DriverCarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumTitle(DriverCarOwnType);
		
		public string DistrictString => District?.DistrictName ?? "";

		public string DriverFullName => string.Join(" ",
			new[] { LastName, Name, Patronymic }
				.Where(x => !string.IsNullOrWhiteSpace(x)));

		public void InitializeEmptyCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = 0, ShortName = "Нет", Name = "Нет" };

			foreach (var day in Days)
			{
				if(day?.CarEventType != null)
				{
					continue;
				}

				if(day != null)
				{
					day.CarEventType = noneEventType;
				}
			}
		}
	}
}
