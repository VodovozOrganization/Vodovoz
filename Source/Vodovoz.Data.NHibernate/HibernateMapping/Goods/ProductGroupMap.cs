using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
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
			Map(x => x.IsArchive).Column("is_archived");
			Map(x => x.CharacteristicsText).Column("characteristics");
			Map(x => x.OnlineStoreExternalId).Column("online_store_external_id");
			Map(x => x.IsHighlightInCarLoadDocument).Column("is_highlight_in_car_load_document");
			Map(x => x.IsNeedAdditionalControl).Column("is_need_additional_control");

			References(x => x.Parent).Column("parent_id");
			References(x => x.OnlineStore).Column("online_store_id");
			HasMany(x => x.Childs).Inverse().Cascade.All().LazyLoad().KeyColumn("parent_id");
		}
	}
}

