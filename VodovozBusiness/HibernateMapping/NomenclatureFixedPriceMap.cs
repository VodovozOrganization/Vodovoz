using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping {
    public class NomenclatureFixedPriceMap : ClassMap<NomenclatureFixedPrice> {
        
        public NomenclatureFixedPriceMap ()
        {
            Table ("additional_agreement_water_fixed_price");

            Id (x => x.Id).Column ("id").GeneratedBy.Native ();
            Map (x => x.FixedPrice).Column ("price");
            
            References (x => x.Nomenclature).Column ("nomenclature_id");
            References (x => x.Counterparty).Column ("counterparty_id");
            References (x => x.DeliveryPoint).Column ("delivery_point_id");
        }
    }
}