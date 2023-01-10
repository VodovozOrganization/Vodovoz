using FluentNHibernate.Mapping;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.HibernateMapping.Suppliers
{
	public class TrueMarkCashReceiptProductCodeMap : ClassMap<TrueMarkCashReceiptProductCode>
	{
		public TrueMarkCashReceiptProductCodeMap()
		{
			Table("true_mark_cash_receipt_product_code");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.TrueMarkCashReceiptOrder).Column("true_mark_cash_receipt_order_id");
			References(x => x.OrderItem).Column("order_item_id");
			Map(x => x.IsDefectiveSourceCode).Column("is_defective_source_code");
			Map(x => x.CodeSource).Column("identification_code_source");
			Map(x => x.CodeResult).Column("identification_code_result");
		}
	}
}
