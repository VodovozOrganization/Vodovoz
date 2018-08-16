using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class ProductGroupMap : ClassMap<ProductGroup>
	{
		public ProductGroupMap()
		{
			Table("nomenclature_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.OnlineStoreGuid).Column("online_store_guid");
			Map(x => x.ExportToOnlineStore).Column("export_to_store");

			References(x => x.Parent).Column("parent_id");
			HasMany(x => x.Childs).Inverse().Cascade.All().LazyLoad().KeyColumn("parent_id");
		}
	}
}

