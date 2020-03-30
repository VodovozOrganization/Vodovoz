using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;
namespace Vodovoz.HibernateMapping.Operations
{
	public class CashlessIncomeOperationMap : ClassMap<CashlessIncomeOperation>
	{
		public CashlessIncomeOperationMap()
		{
			Table("cashless_income_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Sum).Column("sum");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Payment).Column("payment_id");
		}
	}
}
