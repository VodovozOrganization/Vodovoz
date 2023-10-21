using Gamma.Utilities;
using QS.Project.Journal;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Employees
{
	public class EmployeeJournalNode : JournalEntityNodeBase<Employee>
	{
		public override string Title => FullName;
		public string EmpLastName { get; set; }
		public string EmpFirstName { get; set; }
		public string EmpMiddleName { get; set; }
		public string EmployeeComment { get; set; }
		public EmployeeCategory EmpCatEnum { get; set; }
		public EmployeeStatus Status { get; set; }
		public string FullName => $"{EmpLastName} {EmpFirstName} {EmpMiddleName}";
		/// <summary>
		/// Стаж работы в месяцах
		/// </summary>
		public int TotalMonths { get; set; }
		/// <summary>
		/// Средняя з/п за три месяца
		/// </summary>
		public decimal AvgSalary { get; set; }
		public string SubdivisionTitle { get; set; }
		public decimal Balance { get; set; }
	}

	public class EmployeeWithLastWorkingDayJournalNode : EmployeeJournalNode
	{
		public DateTime? LastWorkingDay { get; set; }

		public string LastWorkingDayString => LastWorkingDay?.ToShortDateString();
		public string EmployeeCategoryString => EmpCatEnum.GetEnumTitle();
		public string StatusString => Status.GetEnumTitle();
	}
}
