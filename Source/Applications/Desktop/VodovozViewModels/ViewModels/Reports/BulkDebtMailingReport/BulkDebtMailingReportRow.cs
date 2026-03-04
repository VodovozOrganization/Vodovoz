using System;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingReportRow
	{
		public DateTime ActionDateTime { get; set; }
		public StoredEmailStates State { get; set; }
		public int CounterpartyId { get; set; }
		public string CounterpartyName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string ActionDatetimeString => ActionDateTime.ToString();
		public string StateString => Gamma.Utilities.AttributeUtil.GetEnumTitle(State);
	}
}
