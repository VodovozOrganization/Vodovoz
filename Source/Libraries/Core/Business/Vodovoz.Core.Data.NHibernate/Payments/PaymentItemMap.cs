using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Payments
{
	public class PaymentItemMap : ClassMap<PaymentItemEntity>
	{
		public PaymentItemMap()
		{
			Table("payment_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Sum).Column("sum");
			Map(x => x.PaymentItemStatus).Column("payment_item_status");

			References(x => x.Order).Column("order_id");
			References(x => x.Payment).Column("payment_id");
			References(x => x.CashlessMovementOperation).Column("cashless_movement_operation_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
