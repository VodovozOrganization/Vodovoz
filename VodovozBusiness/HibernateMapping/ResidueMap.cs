using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.HMap
{
	public class ResidueMap : ClassMap<Residue>
	{
		public ResidueMap ()
		{
			Table("doc_residue");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Date).Column ("date");
			Map(x => x.LastEditTime).Column("last_edit_time");
			Map(x => x.BottlesResidue).Column ("bottles_residue");
			Map(x => x.DepositResidueBottels).Column("deposit_residue_bottles");
			Map(x => x.DepositResidueEquipment).Column("deposit_residue_equipment");
			Map(x => x.DebtResidue).Column("money_debt_residue");
			Map(x => x.DebtPaymentType).Column("debt_payment_type").CustomType<PaymentTypeStringType>();
			Map(x => x.Comment).Column("comment");

			References(x => x.Customer).Column("client_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditAuthor).Column("last_edit_author_id");
			References(x => x.BottlesMovementOperation).Column("bottles_movement_operation_id").Cascade.All();
			References(x => x.DepositBottlesOperation).Column("deposit_bottles_operation_id").Cascade.All();
			References(x => x.DepositEquipmentOperation).Column("deposit_equipment_operation_id").Cascade.All();
			References(x => x.MoneyMovementOperation).Column("money_movement_operation_id").Cascade.All();
		}
	}
}

