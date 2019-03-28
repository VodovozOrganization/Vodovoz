using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Sale
{
	public class ScheduleRestriction : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		WeekDayName weekDay;

		public virtual WeekDayName WeekDay {
			get => weekDay;
			set => SetField(ref weekDay, value, () => WeekDay);
		}

		public virtual string SchedulesStr {
			get {
				string result = "";
				foreach(var item in Schedules) {
					if(!string.IsNullOrWhiteSpace(result)) {
						result += ", ";
					}
					result += item.Name;
				}
				return result;
			}
		}

		IList<DeliverySchedule> schedules = new List<DeliverySchedule>();

		[Display(Name = "Графики доставки")]
		public virtual IList<DeliverySchedule> Schedules {
			get => schedules;
			set => SetField(ref schedules, value, () => Schedules);
		}

		GenericObservableList<DeliverySchedule> observableSchedules;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliverySchedule> ObservableSchedules {
			get {
				if(observableSchedules == null) {
					observableSchedules = new GenericObservableList<DeliverySchedule>(Schedules);
				}
				return observableSchedules;
			}
		}

		public virtual void Save(IUnitOfWork UoW)
		{
			UoW.Save(this);
		}

		public virtual void Remove(IUnitOfWork UoW)
		{
			UoW.Delete(this);
		}
	}

	public enum WeekDayName
	{
		[Display(Name = "Сегодня")]
		today = 0,
		[Display(Name = "Понедельник")]
		monday = 1,
		[Display(Name = "Вторник")]
		tuesday = 2,
		[Display(Name = "Среда")]
		wednesday = 3,
		[Display(Name = "Четверг")]
		thursday = 4,
		[Display(Name = "Пятница")]
		friday = 5,
		[Display(Name = "Суббота")]
		saturday = 6,
		[Display(Name = "Воскресенье")]
		sunday = 7
	}

	public class WeekDayNameStringType : NHibernate.Type.EnumStringType
	{
		public WeekDayNameStringType() : base(typeof(WeekDayName)) { }
	}
}
