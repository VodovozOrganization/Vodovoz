using QS.HistoryLog.Domain;
using System;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public class OrderChangesReportRow
	{
		private const string _dateTimeFormatString = "dd.MM.yyyy\nHH:mm";
		public int ChangedEntityId { get; set; }
		public int RowNumber { get; set; }
		public string CounterpartyFullName { get; set; }
		public PersonType CounterpartyPersonType { get; set; }
		public string CounterpartyInn { get; set; }
		public string DriverPhoneComment { get; set; }
		public DateTime? PaymentDate { get; set; }
		public int OrderId { get; set; }
		public decimal? OrderSum { get; set; }
		public DateTime? TimeDelivered { get; set; }
		public DateTime ChangeTime { get; set; }
		public string NomenclatureName { get; set; }
		public string NomenclatureOfficialName { get; set; }
		public EntityChangeOperation ChangeOperation { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public string Driver { get; set; }
		public string Author { get; set; }
		public string SmsNew { get; set; }
		public string QrNew { get; set; }

		public string CounterpartyInfo =>
			CounterpartyPersonType == PersonType.legal
			? string.Concat(CounterpartyFullName, " ", CounterpartyInn)
			: CounterpartyFullName;

		public string PaymentDateString =>
			PaymentDate?.ToString(_dateTimeFormatString);

		public string TimeDeliveredString =>
			TimeDelivered?.ToString(_dateTimeFormatString);

		public string ChangeTimeString =>
			ChangeTime.ToString(_dateTimeFormatString);

		public string OldValueFull
		{
			get
			{
				if(ChangeOperation == EntityChangeOperation.Delete)
				{
					return NomenclatureOfficialName;
				}

				if(OldValue == "Наличная" && SmsNew == "True")
				{
					return "Наличная,\nс оплатой по смс";
				}

				if(OldValue == "Наличная" && QrNew == "True")
				{
					return "Наличная,\nс оплатой по СБП";
				}

				return OldValue;
			}
		}

		public string NewValueFull =>
			ChangeOperation == EntityChangeOperation.Create
			? NomenclatureOfficialName
			: NewValue;
	}
}
