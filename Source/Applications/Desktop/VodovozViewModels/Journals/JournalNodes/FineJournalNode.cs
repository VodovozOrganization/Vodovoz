using System;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;
using Gamma.Utilities;

namespace Vodovoz.Journals.JournalNodes
{
	public class FineJournalNode : JournalEntityNodeBase<Fine>
	{
		public override string Title => FinedEmployeesNames;

		public DateTime Date { get; set; }

		public string FinedEmployeesNames { get; set; }

		public string FineCategoryName { get; set; }

		public decimal FineSum { get; set; }

		public string FineReason { get; set; }

		public string AuthorName { get; set; }

		public string FinedEmployeesSubdivisions { get; set; }

		public string CarEvent { get; set; }
	}
}
