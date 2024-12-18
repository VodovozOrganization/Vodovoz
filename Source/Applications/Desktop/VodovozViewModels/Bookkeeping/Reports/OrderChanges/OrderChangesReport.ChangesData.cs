using QS.HistoryLog.Domain;
using System;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReport
	{
		public class ChangesData
		{
			public int ChangeEntityId { get; set; }
			public int OrderId { get; set; }
			public int OrganizationId { get; set; }
			public string DriversPhoneComment { get; set; }
			public int CounterpartyId { get; set; }
			public int ChangeSetId { get; set; }
			public string EntityClassName { get; set; }
			public int EntityId { get; set; }
			public DateTime ChangeTime { get; set; }
			public EntityChangeOperation ChangeOperation { get; set; }
			public DateTime? ShippedDate { get; set; }
			public FieldChangeType ChangeType { get; set; }
			public string FieldName { get; set; }
			public string OldValue { get; set; }
			public string NewValue { get; set; }
			public string SmsNew { get; set; }
			public string QrNew { get; set; }
		}
	}
}
