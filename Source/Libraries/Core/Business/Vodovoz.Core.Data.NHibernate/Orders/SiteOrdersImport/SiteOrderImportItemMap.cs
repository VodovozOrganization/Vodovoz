using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.SiteOrdersImport;

namespace Vodovoz.Core.Data.NHibernate.Orders.SiteOrdersImport
{
	public class SiteOrderImportItemMap : ClassMap<SiteOrderImportItem>
	{
		public SiteOrderImportItemMap()
		{
			Schema("Vodovoz_old_monitoring");
			Table("site_orders_import_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.SiteOrderId)
				.Column("site_order_id")
				.UniqueKey("uk_site_order_entity");

			Map(x => x.EntityType)
				.Column("entity_type")
				.UniqueKey("uk_site_order_entity");

			Map(x => x.SiteStatus)
				.Column("site_status");

			Map(x => x.SiteUpdatedAt)
				.Column("site_updated_at");

			Map(x => x.BatchId)
				.Column("batch_id");

			Map(x => x.ContractVersion)
				.Column("contract_version");

			Map(x => x.SentAt)
				.Column("sent_at");

			Map(x => x.Payload)
				.Column("payload")
				.Length(int.MaxValue);

			Map(x => x.ReceivedAt)
				.Column("received_at");
		}
	}
}
