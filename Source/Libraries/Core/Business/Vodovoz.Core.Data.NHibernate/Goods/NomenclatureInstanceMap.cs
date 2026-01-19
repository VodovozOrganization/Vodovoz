using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclatureInstanceMap : ClassMap<NomenclatureInstance>
	{
		public NomenclatureInstanceMap()
		{
			Table("nomenclature_instances");

			Id(x => x.Id).GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.PurchasePrice).Column("purchase_price");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
