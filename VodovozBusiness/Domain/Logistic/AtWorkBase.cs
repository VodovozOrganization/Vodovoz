using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	public abstract class AtWorkBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Employee employee;

		[Display(Name = "Водитель")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		private DateTime date;

		[Display(Name = "День")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		private ushort trips = 1;

		[Display(Name = "Количество поездок")]
		[Obsolete("Потом нужно будет удалить")]
		public virtual ushort Trips {
			get { return trips; }
			set { SetField(ref trips, value, () => Trips); }
		}
	}
}
