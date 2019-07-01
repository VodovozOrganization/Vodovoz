using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class ResidueEquipmentDepositItemMap : ClassMap<ResidueEquipmentDepositItem>
	{
		public ResidueEquipmentDepositItemMap()
		{
			Table("doc_residue_equipment_deposit_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count");
			Map(x => x.EquipmentDeposit).Column("deposit");
			Map(x => x.EquipmentDirection).Column("direction").CustomType<ResidueEquipmentDirectionStringType>();

			References(x => x.Residue).Column("residue_id");
			References(x => x.MovementOperation).Column("counterparty_movement_operation_id").Cascade.All();
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
