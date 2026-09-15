using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        Nominative = "График работы водителя",
        NominativePlural = "Графики работы водителя")]
    public class DriverWorkSchedule : PropertyChangedBase, IDomainObject, ICloneable
    {
        public virtual int Id { get; set; }

        private DriverWorkScheduleSet driverWorkScheduleSet;
        [Display(Name = "Версия графиков работы водителя")]
        public virtual DriverWorkScheduleSet DriverWorkScheduleSet {
            get => driverWorkScheduleSet;
            set => SetField(ref driverWorkScheduleSet, value);
        }

        private WeekDayName weekDay;
        [Display(Name = "День недели")]
        public virtual WeekDayName WeekDay {
            get => weekDay;
            set => SetField(ref weekDay, value);
        }

        private DeliveryDaySchedule daySchedule;
        [Display(Name = "График работы")]
        public virtual DeliveryDaySchedule DaySchedule {
            get => daySchedule;
            set => SetField(ref daySchedule, value);
        }
        
        //FIXME Удалить после обновления
        private Employee driver;
        [Display(Name = "Водитель")]
        public virtual Employee Driver {
            get => driver;
            set => SetField(ref driver, value);
        }
        
        public virtual object Clone()
        {
            return new DriverWorkSchedule {
                WeekDay = WeekDay,
                DaySchedule = DaySchedule,
                DriverWorkScheduleSet = DriverWorkScheduleSet
            };
        }
    }
}
