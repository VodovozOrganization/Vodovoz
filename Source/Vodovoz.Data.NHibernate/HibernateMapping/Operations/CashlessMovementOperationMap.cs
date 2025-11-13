using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;
using VodovozBusiness.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class CashlessMovementOperationMap : ClassMap<CashlessMovementOperation>
	{
		public CashlessMovementOperationMap()
		{
			Table("cashless_movement_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Income).Column("income");
			Map(x => x.Expense).Column("expense");
			Map(x => x.CashlessMovementOperationStatus)
				.Column("cashless_movement_operation_status");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Organization).Column("organization_id");
		}
	}
}
