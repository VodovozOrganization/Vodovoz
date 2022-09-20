using QS.Project.Journal;
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
		public string RowColor => Status == EmployeeStatus.IsFired ? "grey" : "black";
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
}
