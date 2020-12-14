using FluentNHibernate.Mapping;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.HibernateMapping
{
    public class CashBoxMap : ClassMap<CashBox>
    {
        public CashBoxMap()
        {
            Table("cash_boxes");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.UserName).Column("user_name");
            Map(x => x.Password).Column("password");
            Map(x => x.RetailPoint).Column("retail_point");
        }
    }
}