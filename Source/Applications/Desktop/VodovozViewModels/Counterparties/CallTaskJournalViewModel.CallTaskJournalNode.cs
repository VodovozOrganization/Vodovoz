using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskJournalViewModel
	{
		public class CallTaskJournalNode : JournalEntityNodeBase<CallTask>
		{
			public override string Title => "";

			public CallTaskStatus TaskStatus { get; set; }
			public string ClientName { get; set; }
			public string AddressName { get; set; }
			public int DebtByAddress { get; set; }
			public int DebtByClient { get; set; }
			public string DeliveryPointPhones { get; set; }
			public string CounterpartyPhones { get; set; }

			public string EmployeeName { get; set; }
			public string EmployeeLastName { get; set; }
			public string EmployeePatronymic { get; set; }

			public string AssignedEmployeeName => PersonHelper.PersonNameWithInitials(EmployeeLastName, EmployeeName, EmployeePatronymic);

			public DateTime Deadline { get; set; }
			public DateTime CreationDate { get; set; }
			public ImportanceDegreeType ImportanceDegree { get; set; }
			public bool IsTaskComplete { get; set; }
			public int TareReturn { get; set; }
			public string Comment { get; set; }
		}
	}
}
