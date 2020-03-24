using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public class DriverWorkSchedule : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private bool atWork = false;

		public virtual bool AtWork {
			get => atWork;
			set => SetField(ref atWork, value);
		}

		private WeekDayName weekDay;

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

		private Employee employee;
		[Display(Name = "Водитель")]
		public virtual Employee Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		public DriverWorkSchedule()
		{
		}
	}
}
