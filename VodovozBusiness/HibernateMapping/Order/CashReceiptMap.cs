using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping
{
	public class CashReceiptMap : ClassMap<CashReceipt>
	{
		public CashReceiptMap()
		{
			Table("cash_receipts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Sent).Column("sent");
			Map(x => x.HttpCode).Column("http_code");
			References(x => x.Order).Column("order_id");
		}
	}
}
