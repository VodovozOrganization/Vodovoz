using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping {
    public class BitrixDealRegistrationMap : ClassMap<BitrixDealRegistration> {
        public BitrixDealRegistrationMap()
        {
            Table("bitrix_deal_registration");
            Id(x => x.Id).Column("id");
            
            Map(x => x.Success).Column("success");
            Map(x => x.CreateDate).Column("created_date");
            Map(x => x.ProcessedDate).Column("processed_date");
            Map(x => x.ErrorDescription).Column("exception_text");
            Map(x => x.BitrixId).Column("bitrix_id");
            Map(x => x.NeedSync).Column("need_sync");

            References(x => x.Order).Column("order_id").Not.LazyLoad();
        }
    }
}
