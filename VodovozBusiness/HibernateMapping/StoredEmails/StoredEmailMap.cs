using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.HibernateMapping.StoredEmails
{
	public class StoredEmailMap : ClassMap<StoredEmail>
	{
		public StoredEmailMap()
		{
			Table("stored_emails");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Order).Column("order_id");
			Map(x => x.DocumentType).Column("document_type");
			Map(x => x.ExternalId).Column("external_id");
			Map(x => x.SendDate).Column("send_date");
			Map(x => x.State).Column("state").CustomType<StoredEmailActionStatesStringType>();
			Map(x => x.StateChangeDate).Column("state_change_date");
			Map(x => x.Description).Column("description");
			Map(x => x.RecipientAddress).Column("recipient_address");
			Map(x => x.ManualSending).Column("manual_sending");
			References(x => x.Author).Column("author_id");
			References(x => x.OrderWithoutShipmentForDebt).Column("bill_ws_for_debt_id");
			References(x => x.OrderWithoutShipmentForPayment).Column("bill_ws_for_payment_id");
			References(x => x.OrderWithoutShipmentForAdvancePayment).Column("bill_ws_for_advance_payment_id");
		}
	}
}
