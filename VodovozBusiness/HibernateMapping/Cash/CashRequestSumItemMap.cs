using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
    public class CashRequestSumItemMap: ClassMap<CashRequestSumItem>
    {
        public CashRequestSumItemMap()
        {
            Table("cash_request_sum_items");
            
            Id(x => x.Id).Column ("id").GeneratedBy.Native();
            
            Map(x => x.Sum).Column ("sum");
            Map(x => x.Date).Column ("date");
            Map(x => x.Comment).Column ("comment");

            //Refs
            References(x => x.AccountableEmployee).Column("accountable_employee_id");
            References(x => x.Expense).Column("expense_id");
            References(x => x.CashRequest).Column("cash_request_id");
        }
    }
}