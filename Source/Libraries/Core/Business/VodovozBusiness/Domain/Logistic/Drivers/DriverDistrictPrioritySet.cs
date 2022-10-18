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
        Nominative = "Версия приоритетов районов водителя",
        NominativePlural = "Версии приоритетов районов водителя")]
    [EntityPermission]
    public class DriverDistrictPrioritySet : PropertyChangedBase, IDomainObject, ICloneable
    {
         public virtual int Id { get; set; }

		private DateTime dateCreated;
		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated
		{
			get => dateCreated;
			set => SetField(ref dateCreated, value);
		}

		private DateTime dateLastChanged;
		[Display(Name = "Время последнего изменения")]
		public virtual DateTime DateLastChanged
		{
			get => dateLastChanged;
			set => SetField(ref dateLastChanged, value);
		}

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

        private DateTime? dateActivated;
        [Display(Name = "Время активации")]
        public virtual DateTime? DateActivated {
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

        private IList<DriverDistrictPriority> driverDistrictPriorities = new List<DriverDistrictPriority>();
        [Display(Name = "Приоритеты районов водителя")]
        public virtual IList<DriverDistrictPriority> DriverDistrictPriorities {
            get => driverDistrictPriorities;
            set => SetField(ref driverDistrictPriorities, value);
        }

        private GenericObservableList<DriverDistrictPriority> observableDriverDistrictPriorities;
        public virtual GenericObservableList<DriverDistrictPriority> ObservableDriverDistrictPriorities =>
            observableDriverDistrictPriorities ?? (observableDriverDistrictPriorities = new GenericObservableList<DriverDistrictPriority>(DriverDistrictPriorities));

        public virtual void CheckAndFixDistrictsPriorities()
        {
            //Сортировка по приоритету
            for(int i = 0; i < DriverDistrictPriorities.Count; i++) {
                for(int j = 0; j < DriverDistrictPriorities.Count - 1 - i; j++) {
                    if(DriverDistrictPriorities[j].Priority > DriverDistrictPriorities[j + 1].Priority) {
                        (DriverDistrictPriorities[j], DriverDistrictPriorities[j + 1]) =
                            (DriverDistrictPriorities[j + 1], DriverDistrictPriorities[j]);
                    }
                }
            }
            for(int i = 0; i < DriverDistrictPriorities.Count; i++) {
                //Удаляем нулы, возможно не нужно
                if(DriverDistrictPriorities[i] == null) {
                    DriverDistrictPriorities.RemoveAt(i);
                    i--;
                    continue;
                }
                //Переприсваиваем приоритеты
                if(DriverDistrictPriorities[i].Priority != i) {
                    DriverDistrictPriorities[i].Priority = i;
                }
            }
            observableDriverDistrictPriorities =
                new GenericObservableList<DriverDistrictPriority>(DriverDistrictPriorities);
        }

        public virtual object Clone()
        {
            var newScheduleSet = new DriverDistrictPrioritySet {
                Driver = Driver,
                DriverDistrictPriorities = new List<DriverDistrictPriority>()
            };
            foreach(DriverDistrictPriority priority in ObservableDriverDistrictPriorities) {
                var newPriority = (DriverDistrictPriority)priority.Clone();
                newPriority.DriverDistrictPrioritySet = newScheduleSet;
                newScheduleSet.DriverDistrictPriorities.Add(newPriority);
            }
            return newScheduleSet;
        }
    }
}
