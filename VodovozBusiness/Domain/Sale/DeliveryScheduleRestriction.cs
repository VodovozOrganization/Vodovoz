using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Domain.Sale
{
    [Appellative(Gender = GrammaticalGender.Neuter,
        NominativePlural = "ограничения времени доставки",
        Nominative = "ограничение времени доставки")]
    public class DeliveryScheduleRestriction : PropertyChangedBase, IDomainObject, ICloneable
    {
        public virtual int Id { get; set; }

        private SectorWeekDayScheduleVersion _sectorWeekDayScheduleVersion;

        public virtual SectorWeekDayScheduleVersion SectorWeekDayScheduleVersion
        {
	        get => _sectorWeekDayScheduleVersion;
	        set => SetField(ref _sectorWeekDayScheduleVersion, value);
        }

        private WeekDayName weekDay;
        [Display(Name = "День недели")]
        public virtual WeekDayName WeekDay {
            get => weekDay;
            set => SetField(ref weekDay, value);
        }

        private DeliverySchedule deliverySchedule;
        [Display(Name = "График доставки")]
        public virtual DeliverySchedule DeliverySchedule {
            get => deliverySchedule;
            set => SetField(ref deliverySchedule, value);
        }
        
        private AcceptBefore acceptBefore;
        [Display(Name = "Прием до")]
        public virtual AcceptBefore AcceptBefore {
            get => acceptBefore;
            set => SetField(ref acceptBefore, value);
        }

        public virtual string AcceptBeforeTitle => AcceptBefore?.Name ?? "";
        public virtual object Clone()
        {
            var newDeliveryScheduleRestriction = new DeliveryScheduleRestriction {
	            DeliverySchedule = DeliverySchedule,
	            AcceptBefore = AcceptBefore,
                WeekDay = WeekDay,
                SectorWeekDayScheduleVersion = SectorWeekDayScheduleVersion
            };
            return newDeliveryScheduleRestriction;
        }

        public override string ToString()
        {
            var acceptBeforeStr = AcceptBefore == null ? "" : ", Время приема до: " + AcceptBeforeTitle;
            return $" День недели: {WeekDay.GetEnumTitle()}, График доставки: {DeliverySchedule.Name}{acceptBeforeStr}";
        }
    }
}
