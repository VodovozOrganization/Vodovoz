using Gamma.Utilities;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingSummaryReportRow
	{
		private string _stateString;
		public StoredEmailStates State { get; set; }
		public string StateString
		{
			get
			{
				if(!string.IsNullOrEmpty(_stateString))
				{
					return _stateString;
				}

				if(State is StoredEmailStates.SendingComplete)
				{
					return "Отправлено, но не открыто";
				}

				return AttributeUtil.GetEnumTitle(State);
			}

			set => _stateString = value;
		}
		public int Count { get; set; }
		public CounterpartyEmailType EmailType { get; set; }
		public string EmailTypeString => AttributeUtil.GetEnumTitle(EmailType);
		public string OrganizationName { get; set; }
	}
}
