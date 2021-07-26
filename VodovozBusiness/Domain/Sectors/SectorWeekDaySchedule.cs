using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDaySchedule: PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }
		
		private SectorWeekDayRulesVersion _sectorWeekDayRulesVersion;

		public SectorWeekDayRulesVersion SectorWeekDayRulesVersion
		{
			get => _sectorWeekDayRulesVersion;
			set => SetField(ref _sectorWeekDayRulesVersion, value);
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

		public object Clone()
		{
			#warning Пересмотреть логику клонирования
			var sectorWeekDayRulesVersion = SectorWeekDayRulesVersion.Clone() as SectorWeekDayRulesVersion;
			var deliveryScheduleClone = DeliverySchedule.Clone() as DeliverySchedule;
			var deliveryScheduleRestrictionClone = DeliveryScheduleRestriction.Clone() as DeliveryScheduleRestriction;
			
			return new SectorWeekDaySchedule
			{
				SectorWeekDayRulesVersion = sectorWeekDayRulesVersion,
				DeliverySchedule = deliveryScheduleClone,
				DeliveryWeekDay = DeliveryWeekDay,
				DeliveryScheduleRestriction = deliveryScheduleRestrictionClone
			};
		}
	}
}