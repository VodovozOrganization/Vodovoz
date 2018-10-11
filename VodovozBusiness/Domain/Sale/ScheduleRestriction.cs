using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Sale
{
	public class ScheduleRestriction : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		WeekDayName weekDay;

		public virtual WeekDayName WeekDay {
			get { return weekDay; }
			set { SetField(ref weekDay, value, () => WeekDay); }
		}

		ScheduleRestrictedDistrict district;

		public virtual ScheduleRestrictedDistrict District{
			get { return district; }
			set { SetField(ref district, value, () => District);}
		}

		DeliverySchedule schedule;

		public virtual DeliverySchedule Schedule{
			get { return schedule; }
			set { SetField(ref schedule, value, () => Schedule);}
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
		[Display(Name = "Понедельник")]
		monday,
		[Display(Name = "Вторник")]
		tuesday,
		[Display(Name = "Среда")]
		wednesday,
		[Display(Name = "Четверг")]
		thursday,
		[Display(Name = "Пятница")]
		friday,
		[Display(Name = "Суббота")]
		saturday,
		[Display(Name = "Воскресенье")]
		sunday
	}

	public class WeekDayNameStringType: NHibernate.Type.EnumStringType
	{
		public WeekDayNameStringType ():base(typeof(WeekDayName))
		{
			
		}
	}
}
