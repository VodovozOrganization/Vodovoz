using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class NomenclatureOnlineCatalogMap : ClassMap<NomenclatureOnlineCatalog>
	{
		public NomenclatureOnlineCatalogMap()
		{
			Table("nomenclatures_online_catalogs");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Type).Column("type").Not.Update().Not.Insert().Access.ReadOnly();
			Map(x => x.Name).Column("name");
			Map(x => x.ExternalId).Column("external_id");
		}
	}
}
