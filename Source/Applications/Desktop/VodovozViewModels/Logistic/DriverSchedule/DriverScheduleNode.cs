using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleNode : PropertyChangedBase
	{
		private CarTypeOfUse _carTypeOfUse;
		private CarOwnType _carOwnType;
		private string _regNumber;
		private string _driverFullName;
		private CarOwnType _driverCarOwnType;
		private string _driverPhone;
		private DistrictsSet _district;
		private int _morningAddress;
		private int _morningBottles;
		private int _eveningAddress;
		private int _eveningBottles;
		private DateTime _lastModifiedDateTime;

		private Dictionary<int, DayScheduleNode> _daySchedules = new Dictionary<int, DayScheduleNode>();

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

		public virtual string DriverFullName
		{
			get => _driverFullName;
			set => SetField(ref _driverFullName, value);
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

		public virtual DistrictsSet District
		{
			get => _district;
			set => SetField(ref _district, value);
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

		public virtual Dictionary<int, DayScheduleNode> DaySchedules
		{
			get => _daySchedules;
			set => SetField(ref _daySchedules, value);
		}

		public string LastModifiedDateTimeString => LastModifiedDateTime.ToString();

		public string CarTypeOfUseString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarTypeOfUse);

		public string CarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(CarOwnType);

		public string DriverCarOwnTypeString => Gamma.Utilities.AttributeUtil.GetEnumShortTitle(DriverCarOwnType);
	}
}
