using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class TransferOperationDocumentMap: ClassMap<TransferOperationDocument>
	{
		public TransferOperationDocumentMap()
		{
			Table("transfer_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TimeStamp).Column("operation_time");
			Map(x => x.Comment).Column("comment");

			References(x => x.FromClient).Column("from_client_id");
			References(x => x.FromDeliveryPoint).Column("from_delivery_point_id");
			References(x => x.ToClient).Column("to_client_id");
			References(x => x.ToDeliveryPoint).Column("to_delivery_point_id");

			References(x => x.OutBottlesOperation).Column("outgoing_bottle_operation").Cascade.All();
			References(x => x.IncBottlesOperation).Column("incoming_bottle_operation").Cascade.All();
			References(x => x.OutBottlesDepositOperation).Column("outgoing_bottle_deposit_operation").Cascade.All();
			References(x => x.IncBottlesDepositOperation).Column("incoming_bottle_deposit_operation").Cascade.All();
			References(x => x.OutEquipmentDepositOperation).Column("outgoing_equipment_deposit_operation").Cascade.All();
			References(x => x.IncEquipmentDepositOperation).Column("incoming_equipment_deposit_operation").Cascade.All();


			References(x => x.Author).Column("author_id");
			References(x => x.LastEditor).Column("last_edit_author_id");
			References(x => x.ResponsiblePerson).Column("responsible_person_id");

		}
	}
}
