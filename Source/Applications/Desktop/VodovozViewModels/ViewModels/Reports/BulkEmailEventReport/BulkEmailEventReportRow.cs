using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport
{
	public class BulkEmailEventReportRow
	{
		public DateTime ActionDateTime { get; set; }
		public int CounterpartyId { get; set; }
		public string CounterpartyName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string ActionDatetimeString => ActionDateTime.ToString("g");
		public BulkEmailEventType BulkEmailEventType { get; set; }
		public string BulkEmailEventTypeString => BulkEmailEventType.GetEnumDisplayName();
		public CounterpartyEmailType? EmailType { get; set; }
		public string EmailTypeString => EmailType?.GetEnumDisplayName() ?? string.Empty;
		public string Reason { get; set; }
		public string OtherReason { get; set; }

		public string FullReasonString
		{
			get
			{
				var reason = Reason;

				if(!string.IsNullOrWhiteSpace(OtherReason))
				{
					reason += $": {OtherReason}";
				}

				return reason;
			}
		}

		public string SelectedFilters { get; set; }
	}
}
