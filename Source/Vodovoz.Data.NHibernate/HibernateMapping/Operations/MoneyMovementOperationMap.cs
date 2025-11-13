using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class MoneyMovementOperationMap : ClassMap<MoneyMovementOperation>
	{
		public MoneyMovementOperationMap()
		{
			Table("money_movement_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Debt).Column("debt");
			Map(x => x.Money).Column("money");
			Map(x => x.Deposit).Column("deposit");
			Map(x => x.PaymentType).Column("payment_type");
			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}

