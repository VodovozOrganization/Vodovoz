using FluentNHibernate.Mapping;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.HibernateMapping.Suppliers
{
	/*public class TrueMarkCashReceiptOrderMap : ClassMap<TrueMarkCashReceiptOrder>
	{
		public TrueMarkCashReceiptOrderMap()
		{
			Table("true_mark_cash_receipt_order");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Not.LazyLoad().Column("order_id");
			Map(x => x.Date).Column("date");
			Map(x => x.Status).Column("status");
			References(x => x.CashReceipt).Not.LazyLoad().Column("cash_receipt_id");
			Map(x => x.UnscannedCodesReason).Column("unscanned_codes_reason");
			Map(x => x.ErrorDescription).Column("error_description");

			HasMany(x => x.ScannedCodes).Cascade.AllDeleteOrphan().Not.LazyLoad().Inverse()
				.KeyColumn("true_mark_cash_receipt_order_id");
		}
	}*/
}
