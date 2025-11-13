using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Store
{
	public class WarehouseMap : ClassMap<Warehouse>
	{
		public WarehouseMap()
		{
			Table("warehouses");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.CanReceiveBottles).Column("can_receive_bottles");
			Map(x => x.CanReceiveEquipment).Column("can_receive_equipment");
			Map(x => x.PublishOnlineStore).Column("publish_online_store");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.TypeOfUse).Column("type_of_use");
			Map(x => x.Address).Column("address");

			Map(x => x.OwningSubdivisionId).Column("owning_subdivision");
			Map(x => x.MovementDocumentsNotificationsSubdivisionRecipientId)
				.Column("movement_documents_notifications_subdivision_recipient_id");
		}
	}
}

