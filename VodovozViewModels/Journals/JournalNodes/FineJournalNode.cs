using System;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Journals.JournalNodes
{
	public class FineJournalNode : JournalEntityNodeBase<Fine>
	{
		public override string Title => EmployeesName;

		public DateTime Date { get; set; }

		public string EmployeesName { get; set; }

		public string FineReason { get; set; }

		public decimal FineSumm { get; set; }
	}
}
