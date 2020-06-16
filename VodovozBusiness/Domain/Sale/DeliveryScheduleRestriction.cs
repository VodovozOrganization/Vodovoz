using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Sale
{
    public class DeliveryScheduleRestriction : PropertyChangedBase, IDomainObject
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
    }
}