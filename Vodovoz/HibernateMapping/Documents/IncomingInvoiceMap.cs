using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class IncomingInvoiceMap : ClassMap<IncomingInvoice>
	{
		public IncomingInvoiceMap ()
		{
			Table ("incoming_invoice");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map (x => x.InvoiceNumber).Column ("invoice_number");
			Map (x => x.WaybillNumber).Column ("waybill_number");
			References (x => x.Contractor).Column ("counterparty_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("incoming_invoice_id");
		}
	}
}