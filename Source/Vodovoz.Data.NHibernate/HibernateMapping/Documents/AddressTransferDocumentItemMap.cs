using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class AddressTransferDocumentItemMap : ClassMap<AddressTransferDocumentItem>
	{
		public AddressTransferDocumentItemMap()
		{
			Table("address_transfer_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.AddressTransferType).Column("address_transfer_type");

			References(x => x.Document).Column("address_transfer_document_id");
			References(x => x.OldAddress).Column("old_address_id");
			References(x => x.NewAddress).Column("new_address_id");

			HasMany(x => x.DriverNomenclatureTransferDocumentItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("address_transfer_document_item_id");

			HasMany(x => x.DeliveryFreeBalanceTransferItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("address_transfer_document_item_id");
		}
	}
}
