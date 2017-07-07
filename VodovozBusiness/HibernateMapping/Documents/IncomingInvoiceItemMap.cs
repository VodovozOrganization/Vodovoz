using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class IncomingInvoiceItemMap : ClassMap<IncomingInvoiceItem>
	{
		public IncomingInvoiceItemMap ()
		{
			Table ("store_incoming_invoice_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Price).Column ("price");
			Map (x => x.Amount).Column ("amount");
			Map (x => x.VAT).Column ("vat").CustomType<VATStringType> ();
			References (x => x.Document).Column ("incoming_invoice_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.IncomeGoodsOperation).Column ("good_move_operation_id").Not.Nullable ().Cascade.All ();
		}
	}
}