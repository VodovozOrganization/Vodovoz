using FluentNHibernate.Mapping;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.HibernateMapping
{
    public class CashMachineMap : ClassMap<CashMachine>
    {
        public CashMachineMap()
        {
            Table("cash_machines");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.UserName).Column("user_name");
            Map(x => x.Password).Column("password");
            Map(x => x.BaseAddress).Column("base_address");
        }
    }
}