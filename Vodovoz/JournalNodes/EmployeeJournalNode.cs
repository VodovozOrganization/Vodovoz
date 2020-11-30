using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.JournalNodes
{
	public class EmployeeJournalNode : JournalEntityNodeBase<Employee>
	{
		public override string Title => FullName;
		public string EmpLastName { get; set; }
		public string EmpFirstName { get; set; }
		public string EmpMiddleName { get; set; }
		public EmployeeCategory EmpCatEnum { get; set; }
		public EmployeeStatus Status { get; set; }
		public string FullName => $"{EmpLastName} {EmpFirstName} {EmpMiddleName}";
		public string RowColor => Status == EmployeeStatus.IsFired ? "grey" : "black";
	}
}