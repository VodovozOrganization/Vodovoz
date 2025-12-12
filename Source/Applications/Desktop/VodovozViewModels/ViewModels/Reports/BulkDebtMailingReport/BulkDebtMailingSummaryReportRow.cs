using System;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingSummaryReportRow
	{
		public DateTime ActionDateTime { get; set; }
		public StoredEmailStates State { get; set; }
		public string ActionDatetimeString => ActionDateTime.ToString("d");
		public string StateString => Gamma.Utilities.AttributeUtil.GetEnumTitle(State);
		public int Count { get; set; }
	}
}
