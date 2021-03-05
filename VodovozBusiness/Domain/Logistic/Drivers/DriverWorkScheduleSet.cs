using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        Nominative = "Версия графиков работы водителя",
        NominativePlural = "Версии графиков работы водителя")]
    [EntityPermission]
    public class DriverWorkScheduleSet : PropertyChangedBase, IDomainObject, ICloneable
    {
        public virtual int Id { get; set; }
        
        private Employee driver;
        [Display(Name = "Водитель")]
        public virtual Employee Driver {
            get => driver;
            set => SetField(ref driver, value);
        }
        
        private Employee author;
        [Display(Name = "Автор")]
        public virtual Employee Author {
            get => author;
            set => SetField(ref author, value);
        }
        
        private Employee lastEditor;
        [Display(Name = "Изменил")]
        public virtual Employee LastEditor {
            get => lastEditor;
            set => SetField(ref lastEditor, value);
        }

        private DateTime dateActivated;
        [Display(Name = "Время активации")]
        public virtual DateTime DateActivated {
            get => dateActivated;
            set => SetField(ref dateActivated, value);
        }

        private DateTime? dateDeactivated;
        [Display(Name = "Время деактивации")]
        public virtual DateTime? DateDeactivated {
            get => dateDeactivated;
            set => SetField(ref dateDeactivated, value);
        }

        private bool isActive;
        [Display(Name = "Версия активирована")]
        public virtual bool IsActive {
            get => isActive;
            set => SetField(ref isActive, value);
        }
        
        private bool isCreatedAutomatically;
        [Display(Name = "Создана автоматически")]
        public virtual bool IsCreatedAutomatically {
            get => isCreatedAutomatically;
            set => SetField(ref isCreatedAutomatically, value);
        }

        private IList<DriverWorkSchedule> driverWorkSchedules = new List<DriverWorkSchedule>();
        [Display(Name = "Графики работы водителя")]
        public virtual IList<DriverWorkSchedule> DriverWorkSchedules {
            get => driverWorkSchedules;
            set => SetField(ref driverWorkSchedules, value);
        }

        private GenericObservableList<DriverWorkSchedule> observableDriverWorkSchedules;
        public virtual GenericObservableList<DriverWorkSchedule> ObservableDriverWorkSchedules =>
            observableDriverWorkSchedules ?? (observableDriverWorkSchedules = new GenericObservableList<DriverWorkSchedule>(DriverWorkSchedules));

        public virtual object Clone()
        {
            var newScheduleSet = new DriverWorkScheduleSet {
                Driver = Driver,
                DriverWorkSchedules = new List<DriverWorkSchedule>()
            };
            foreach(DriverWorkSchedule schedule in ObservableDriverWorkSchedules) {
                var newSchedule = (DriverWorkSchedule)schedule.Clone();
                newSchedule.DriverWorkScheduleSet = newScheduleSet;
                newScheduleSet.DriverWorkSchedules.Add(newSchedule);
            }
            return newScheduleSet;
        }
    }
}
