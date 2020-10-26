using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.HibernateMapping.Payments
{
    public class PaymentReturnMap : ClassMap<PaymentReturn>
    {
        public PaymentReturnMap()
        {
            Table("payment_return");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Sum).Column("sum");

            References(x => x.Order).Column("order_id");
            References(x => x.CashlessMovementOperation).Column("cashless_movement_operation_id").Cascade.SaveUpdate();
        }
    }
}