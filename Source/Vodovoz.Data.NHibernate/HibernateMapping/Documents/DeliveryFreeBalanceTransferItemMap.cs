using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class DeliveryFreeBalanceTransferItemMap : ClassMap<DeliveryFreeBalanceTransferItem>
	{
		public DeliveryFreeBalanceTransferItemMap()
		{
			Table("delivery_free_balance_transfer_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");

			References(x => x.AddressTransferDocumentItem).Column("address_transfer_document_item_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.RouteListFrom).Column("route_list_from_id");
			References(x => x.RouteListTo).Column("route_list_to_id");
			References(x => x.DeliveryFreeBalanceOperationFrom)
				.Cascade.All().Column("delivery_free_balance_operation_from_id");
			References(x => x.DeliveryFreeBalanceOperationTo)
				.Cascade.All().Column("delivery_free_balance_operation_to_id");
		}
	}
}
