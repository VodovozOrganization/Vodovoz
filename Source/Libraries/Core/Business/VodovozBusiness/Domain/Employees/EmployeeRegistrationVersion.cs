using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия оформления сотрудника",
		NominativePlural = "версии оформлений сотрудников")]
	[HistoryTrace]
	public class EmployeeRegistrationVersion : PropertyChangedBase, IDomainObject
	{
		private EmployeeRegistration _employeeRegistration;
		private Employee _employee;
		private DateTime _startDate;
		private DateTime? _endDate;
		
		public virtual int Id { get; set; }

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Вид оформления")]
		public virtual EmployeeRegistration EmployeeRegistration
		{
			get => _employeeRegistration;
			set => SetField(ref _employeeRegistration, value);
		}
		
		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}
	}
}
