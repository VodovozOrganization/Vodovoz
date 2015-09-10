using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class IncomingInvoiceItemMap : ClassMap<IncomingInvoiceItem>
	{
		public IncomingInvoiceItemMap ()
		{
			Table ("incoming_invoice_items");
			Not.LazyLoad ();

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