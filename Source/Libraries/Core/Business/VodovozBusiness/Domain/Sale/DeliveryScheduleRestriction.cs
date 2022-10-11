using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "ограничение времени доставки района",
		NominativePlural = "ограничения времени доставки района")]
	[EntityPermission]
    public class DeliveryScheduleRestriction : PropertyChangedBase, IDomainObject, ICloneable
    {
        public virtual int Id { get; set; }

        private District district;
        [Display(Name = "Район")]
        public virtual District District {
            get => district;
            set => SetField(ref district, value, () => District);
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
