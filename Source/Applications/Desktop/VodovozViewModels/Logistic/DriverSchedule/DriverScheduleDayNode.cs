using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleDayNode : PropertyChangedBase
	{
		private DateTime _date;
		private CarEventType _carEventType;
		private int _morningAddress;
		private int _morningBottles;
		private int _eveningAddress;
		private int _eveningBottles;
		private DriverScheduleNode _parentNode;

		public virtual DriverScheduleNode ParentNode
		{
			get => _parentNode;
			set => SetField(ref _parentNode, value);
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
	}
}

