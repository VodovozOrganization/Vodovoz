using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class InventoryNomenclatureInstanceMap : SubclassMap<InventoryNomenclatureInstance>
	{
		public InventoryNomenclatureInstanceMap()
		{
			DiscriminatorValue("InventoryNomenclatureInstance");

			Map(x => x.InventoryNumber).Column("inventory_number");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
