using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	public abstract class AtWorkBase : PropertyChangedBase, IDomainObject
	{
		private Employee _employee;
		private DateTime _date;

		public virtual int Id { get; set; }

		[Display(Name = "Водитель")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		[Display(Name = "День")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}
	}
}
