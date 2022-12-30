using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class NomenclatureInstanceMap : ClassMap<NomenclatureInstance>
	{
		public NomenclatureInstanceMap()
		{
			Table("nomenclature_instances");

			Id(x => x.Id).GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");
			
			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.CostPrice).Column("cost_price");
			Map(x => x.InnerDeliveryPrice).Column("inner_delivery_price");
			Map(x => x.PurchasePrice).Column("purchase_price");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
