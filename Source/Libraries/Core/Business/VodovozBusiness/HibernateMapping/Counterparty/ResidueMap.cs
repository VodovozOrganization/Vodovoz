using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
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
			Map(x => x.BottlesDeposit).Column("deposit_residue_bottles");
			Map(x => x.DebtResidue).Column("money_debt_residue");
			Map(x => x.DebtPaymentType).Column("debt_payment_type").CustomType<PaymentTypeStringType>();
			Map(x => x.Comment).Column("comment");

			References(x => x.Customer).Column("client_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditAuthor).Column("last_edit_author_id");
			References(x => x.BottlesMovementOperation).Column("bottles_movement_operation_id").Cascade.All();
			References(x => x.BottlesDepositOperation).Column("deposit_bottles_operation_id").Cascade.All();
			References(x => x.EquipmentDepositOperation).Column("deposit_equipment_operation_id").Cascade.All();
			References(x => x.MoneyMovementOperation).Column("money_movement_operation_id").Cascade.All();

			HasMany(x => x.EquipmentDepositItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("residue_id");
		}
	}
}

