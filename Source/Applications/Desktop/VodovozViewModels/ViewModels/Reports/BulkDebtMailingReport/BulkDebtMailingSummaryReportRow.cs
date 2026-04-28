using Gamma.Utilities;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingSummaryReportRow
	{
		public StoredEmailStates State { get; set; }
		public string StateString => AttributeUtil.GetEnumTitle(State);
		public int Count { get; set; }
	}
}
