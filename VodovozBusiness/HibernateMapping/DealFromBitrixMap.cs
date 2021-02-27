using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping {
    public class DealFromBitrixMap : ClassMap<DealFromBitrix> {
        public DealFromBitrixMap()
        {
            Table("deals_from_bitrix");
            Id(x => x.Id).Column("id");
            
            Map(x => x.Success).Column("success");
            Map(x => x.CreateDate).Column("created_date");
            Map(x => x.ProcessedDate).Column("processed_date");
            Map(x => x.ExtensionText).Column("exception_text");
            Map(x => x.BitrixId).Column("bitrix_id");

            References(x => x.Order).Column("order_id").Not.LazyLoad();

        }
    }
}