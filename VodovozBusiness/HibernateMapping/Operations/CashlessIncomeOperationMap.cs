using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;
namespace Vodovoz.HibernateMapping.Operations
{
	public class CashlessIncomeOperationMap : ClassMap<CashlessIncomeOperation>
	{
		public CashlessIncomeOperationMap()
		{
			Table("cashless_movement_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Income).Column("income");
			Map(x => x.Expense).Column("expense");

			References(x => x.PaymentItem).Column("payment_item_id");
			References(x => x.Payment).Column("payment_id");
		}
	}
}
