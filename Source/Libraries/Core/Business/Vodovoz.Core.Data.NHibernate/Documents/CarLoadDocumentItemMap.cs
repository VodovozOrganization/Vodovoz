using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class CarLoadDocumentItemMap : ClassMap<CarLoadDocumentItemEntity>
	{
		public CarLoadDocumentItemMap()
		{
			Table("store_car_load_document_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.IsIndividualSetForOrder).Column("is_individual_set_for_order");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Document).Column("car_load_document_id");

			HasMany(x => x.TrueMarkCodes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("car_load_document_item_id");
		}
	}
}
