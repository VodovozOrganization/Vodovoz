using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleDatasetNode : PropertyChangedBase
	{
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
		private DateTime _deliveryDate;
		private int _morningAddress;
		private int _morningBottles;
		private int _eveningAddress;
		private int _eveningBottles;
		private DateTime _lastModifiedDateTime;
		private string _comment;

		private IList<DayScheduleNode> _days = new List<DayScheduleNode>();

		[Display(Name = "Дни расписания")]
		public virtual IList<DayScheduleNode> Days
		{
			get => _days;
			set => SetField(ref _days, value);
		}

		private Dictionary<int, DayScheduleNode> _daysByIndex = new Dictionary<int, DayScheduleNode>();

		[Display(Name = "Дни расписания")]
		public virtual Dictionary<int, DayScheduleNode> DaysByIndex
		{
			get => _daysByIndex;
			set => SetField(ref _daysByIndex, value);
		}

		public void InitializeDays(List<DateTime> weekDays)
		{
			DaysByIndex = new Dictionary<int, DayScheduleNode>();
			for(int i = 0; i < weekDays.Count && i < 7; i++)
			{
				DaysByIndex[i] = new DayScheduleNode { Date = weekDays[i], ParentNode = this };
			}
		}

		#region Weekdays
		// Monday
		private CarEventType _mondayCarEventType;
		private int _mondayMorningAddress;
		private int _mondayMorningBottles;
		private int _mondayEveningAddress;
		private int _mondayEveningBottles;

		// Tuesday
		private CarEventType _tuesdayCarEventType;
		private int _tuesdayMorningAddress;
		private int _tuesdayMorningBottles;
		private int _tuesdayEveningAddress;
		private int _tuesdayEveningBottles;

		// Wednesday
		private CarEventType _wednesdayCarEventType;
		private int _wednesdayMorningAddress;
		private int _wednesdayMorningBottles;
		private int _wednesdayEveningAddress;
		private int _wednesdayEveningBottles;

		// Thursday
		private CarEventType _thursdayCarEventType;
		private int _thursdayMorningAddress;
		private int _thursdayMorningBottles;
		private int _thursdayEveningAddress;
		private int _thursdayEveningBottles;

		// Friday
		private CarEventType _fridayCarEventType;
		private int _fridayMorningAddress;
		private int _fridayMorningBottles;
		private int _fridayEveningAddress;
		private int _fridayEveningBottles;

		// Saturday
		private CarEventType _saturdayCarEventType;
		private int _saturdayMorningAddress;
		private int _saturdayMorningBottles;
		private int _saturdayEveningAddress;
		private int _saturdayEveningBottles;

		// Sunday
		private CarEventType _sundayCarEventType;
		private int _sundayMorningAddress;
		private int _sundayMorningBottles;
		private int _sundayEveningAddress;
		private int _sundayEveningBottles;

		// Monday properties
		public virtual CarEventType MondayCarEventType
		{
			get => _mondayCarEventType;
			set => SetField(ref _mondayCarEventType, value);
		}

		public virtual int MondayMorningAddress
		{
			get => _mondayMorningAddress;
			set => SetField(ref _mondayMorningAddress, value);
		}

		public virtual int MondayMorningBottles
		{
			get => _mondayMorningBottles;
			set => SetField(ref _mondayMorningBottles, value);
		}

		public virtual int MondayEveningAddress
		{
			get => _mondayEveningAddress;
			set => SetField(ref _mondayEveningAddress, value);
		}

		public virtual int MondayEveningBottles
		{
			get => _mondayEveningBottles;
			set => SetField(ref _mondayEveningBottles, value);
		}

		// Tuesday properties
		public virtual CarEventType TuesdayCarEventType
		{
			get => _tuesdayCarEventType;
			set => SetField(ref _tuesdayCarEventType, value);
		}

		public virtual int TuesdayMorningAddress
		{
			get => _tuesdayMorningAddress;
			set => SetField(ref _tuesdayMorningAddress, value);
		}

		public virtual int TuesdayMorningBottles
		{
			get => _tuesdayMorningBottles;
			set => SetField(ref _tuesdayMorningBottles, value);
		}

		public virtual int TuesdayEveningAddress
		{
			get => _tuesdayEveningAddress;
			set => SetField(ref _tuesdayEveningAddress, value);
		}

		public virtual int TuesdayEveningBottles
		{
			get => _tuesdayEveningBottles;
			set => SetField(ref _tuesdayEveningBottles, value);
		}

		// Wednesday properties
		public virtual CarEventType WednesdayCarEventType
		{
			get => _wednesdayCarEventType;
			set => SetField(ref _wednesdayCarEventType, value);
		}

		public virtual int WednesdayMorningAddress
		{
			get => _wednesdayMorningAddress;
			set => SetField(ref _wednesdayMorningAddress, value);
		}

		public virtual int WednesdayMorningBottles
		{
			get => _wednesdayMorningBottles;
			set => SetField(ref _wednesdayMorningBottles, value);
		}

		public virtual int WednesdayEveningAddress
		{
			get => _wednesdayEveningAddress;
			set => SetField(ref _wednesdayEveningAddress, value);
		}

		public virtual int WednesdayEveningBottles
		{
			get => _wednesdayEveningBottles;
			set => SetField(ref _wednesdayEveningBottles, value);
		}

		// Thursday properties
		public virtual CarEventType ThursdayCarEventType
		{
			get => _thursdayCarEventType;
			set => SetField(ref _thursdayCarEventType, value);
		}

		public virtual int ThursdayMorningAddress
		{
			get => _thursdayMorningAddress;
			set => SetField(ref _thursdayMorningAddress, value);
		}

		public virtual int ThursdayMorningBottles
		{
			get => _thursdayMorningBottles;
			set => SetField(ref _thursdayMorningBottles, value);
		}

		public virtual int ThursdayEveningAddress
		{
			get => _thursdayEveningAddress;
			set => SetField(ref _thursdayEveningAddress, value);
		}

		public virtual int ThursdayEveningBottles
		{
			get => _thursdayEveningBottles;
			set => SetField(ref _thursdayEveningBottles, value);
		}

		// Friday properties
		public virtual CarEventType FridayCarEventType
		{
			get => _fridayCarEventType;
			set => SetField(ref _fridayCarEventType, value);
		}

		public virtual int FridayMorningAddress
		{
			get => _fridayMorningAddress;
			set => SetField(ref _fridayMorningAddress, value);
		}

		public virtual int FridayMorningBottles
		{
			get => _fridayMorningBottles;
			set => SetField(ref _fridayMorningBottles, value);
		}

		public virtual int FridayEveningAddress
		{
			get => _fridayEveningAddress;
			set => SetField(ref _fridayEveningAddress, value);
		}

		public virtual int FridayEveningBottles
		{
			get => _fridayEveningBottles;
			set => SetField(ref _fridayEveningBottles, value);
		}

		// Saturday properties
		public virtual CarEventType SaturdayCarEventType
		{
			get => _saturdayCarEventType;
			set => SetField(ref _saturdayCarEventType, value);
		}

		public virtual int SaturdayMorningAddress
		{
			get => _saturdayMorningAddress;
			set => SetField(ref _saturdayMorningAddress, value);
		}

		public virtual int SaturdayMorningBottles
		{
			get => _saturdayMorningBottles;
			set => SetField(ref _saturdayMorningBottles, value);
		}

		public virtual int SaturdayEveningAddress
		{
			get => _saturdayEveningAddress;
			set => SetField(ref _saturdayEveningAddress, value);
		}

		public virtual int SaturdayEveningBottles
		{
			get => _saturdayEveningBottles;
			set => SetField(ref _saturdayEveningBottles, value);
		}

		// Sunday properties
		public virtual CarEventType SundayCarEventType
		{
			get => _sundayCarEventType;
			set => SetField(ref _sundayCarEventType, value);
		}

		public virtual int SundayMorningAddress
		{
			get => _sundayMorningAddress;
			set => SetField(ref _sundayMorningAddress, value);
		}

		public virtual int SundayMorningBottles
		{
			get => _sundayMorningBottles;
			set => SetField(ref _sundayMorningBottles, value);
		}

		public virtual int SundayEveningAddress
		{
			get => _sundayEveningAddress;
			set => SetField(ref _sundayEveningAddress, value);
		}

		public virtual int SundayEveningBottles
		{
			get => _sundayEveningBottles;
			set => SetField(ref _sundayEveningBottles, value);
		}

		#endregion


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

		public virtual DateTime DeliveryDate
		{
			get => _deliveryDate;
			set => SetField(ref _deliveryDate, value);
		}

		public virtual int MorningAddress
		{
			get => _morningAddress;
			set => SetField(ref _morningAddress, value);
		}

		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		public virtual int EveningAddress
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

		public string LastModifiedDateTimeString => LastModifiedDateTime.ToString();

		public string CarTypeOfUseString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarTypeOfUse);

		public string CarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarOwnType);

		public string DriverCarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumTitle(DriverCarOwnType);
		
		public string DistrictString => District?.DistrictName ?? "";

		public string DriverFullName => string.Join(" ",
			new[] { LastName, Name, Patronymic }
				.Where(x => !string.IsNullOrWhiteSpace(x)));

		// Костыль, чтобы в treeView отображалось "Нет" у null значений.
		public void InitializeEmptyCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = 0, ShortName = "Нет", Name = "Нет" };

			if(MondayCarEventType == null)
			{
				MondayCarEventType = noneEventType;
			}

			if(TuesdayCarEventType == null)
			{
				TuesdayCarEventType = noneEventType;
			}

			if(WednesdayCarEventType == null)
			{
				WednesdayCarEventType = noneEventType;
			}

			if(ThursdayCarEventType == null)
			{
				ThursdayCarEventType = noneEventType;
			}

			if(FridayCarEventType == null)
			{
				FridayCarEventType = noneEventType;
			}

			if(SaturdayCarEventType == null)
			{
				SaturdayCarEventType = noneEventType;
			}

			if(SundayCarEventType == null)
			{
				SundayCarEventType = noneEventType;
			}
		}
	}
}
