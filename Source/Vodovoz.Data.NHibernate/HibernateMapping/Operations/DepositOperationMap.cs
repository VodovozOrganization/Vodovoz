using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class DepositOperationMap : ClassMap<DepositOperation>
	{
		public DepositOperationMap()
		{
			Table("deposit_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.ReceivedDeposit).Column("received_deposit");
			Map(x => x.RefundDeposit).Column("refund_deposit");
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.DepositType).Column("deposit_type");
			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
		}
	}
}

