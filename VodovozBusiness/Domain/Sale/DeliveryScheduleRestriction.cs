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

        private Sector _sector;
        [Display(Name = "Район")]
        public virtual Sector Sector {
            get => _sector;
            set => SetField(ref _sector, value, () => Sector);
        }
        
        private WeekDayName weekDay;
        [Display(Name = "День недели")]
        public virtual WeekDayName WeekDay {
            get => weekDay;
            set => SetField(ref weekDay, value, () => WeekDay);
        }

        private DeliverySchedule deliverySchedule;
        [Display(Name = "График доставки")]
        public virtual DeliverySchedule DeliverySchedule {
            get => deliverySchedule;
            set => SetField(ref deliverySchedule, value, () => DeliverySchedule);
        }
        
        private AcceptBefore acceptBefore;
        [Display(Name = "Прием до")]
        public virtual AcceptBefore AcceptBefore {
            get => acceptBefore;
            set => SetField(ref acceptBefore, value, () => AcceptBefore);
        }

        public virtual string AcceptBeforeTitle => AcceptBefore?.Name ?? "";
        public virtual object Clone()
        {
            var newDeliveryScheduleRestriction = new DeliveryScheduleRestriction {
                AcceptBefore = AcceptBefore,
                DeliverySchedule = DeliverySchedule,
                WeekDay = WeekDay
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