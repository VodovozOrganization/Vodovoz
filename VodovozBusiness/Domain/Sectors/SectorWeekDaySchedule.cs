using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDaySchedule: PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }
		
		private DateTime _startDate;
		
		[Display(Name = "Время создания")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime _endDate;

		[Display(Name = "Время закрытия")]
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		private SectorWeekDayScheduleVersion _sectorWeekDayScheduleVersion;

		public SectorWeekDayScheduleVersion SectorWeekDayScheduleVersion
		{
			get => _sectorWeekDayScheduleVersion;
			set => SetField(ref _sectorWeekDayScheduleVersion, value);
		}

		private WeekDayName _deliveryWeekDay;

		public WeekDayName DeliveryWeekDay
		{
			get => _deliveryWeekDay;
			set => SetField(ref _deliveryWeekDay, value);
		}

		private DeliverySchedule _deliverySchedule;

		public DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule,value);
		}

		private DeliveryScheduleRestriction _deliveryScheduleRestriction;

		public DeliveryScheduleRestriction DeliveryScheduleRestriction
		{
			get => _deliveryScheduleRestriction;
			set => SetField(ref _deliveryScheduleRestriction, value);
		}

		public SectorWeekDaySchedule()
		{
			DeliverySchedule = new DeliverySchedule();
			DeliveryScheduleRestriction = new DeliveryScheduleRestriction();
		}

		public object Clone()
		{
			#warning Пересмотреть логику клонирования
			var sectorWeekDayScheduleVersionClone = SectorWeekDayScheduleVersion.Clone() as SectorWeekDayScheduleVersion;
			var deliveryScheduleClone = DeliverySchedule.Clone() as DeliverySchedule;
			var deliveryScheduleRestrictionClone = DeliveryScheduleRestriction.Clone() as DeliveryScheduleRestriction;
			
			return new SectorWeekDaySchedule
			{
				StartDate = StartDate,
				EndDate = EndDate,
				SectorWeekDayScheduleVersion = sectorWeekDayScheduleVersionClone,
				DeliverySchedule = deliveryScheduleClone,
				DeliveryWeekDay = DeliveryWeekDay,
				DeliveryScheduleRestriction = deliveryScheduleRestrictionClone
			};
		}
	}
}