using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class CarUnderloadDocumentItemMap : ClassMap<CarUnderloadDocumentItem>
    {
        public CarUnderloadDocumentItemMap()
        {
            Table ("store_car_underload_document_items");
			
            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Amount).Column("amount");

            References(x => x.CarUnderloadDocument).Column("car_underload_document_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.DeliveryFreeBalanceOperation).Cascade.All().Column("delivery_free_balance_operation_id");
		}
    }
}
