using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.BasicHandbooks;

namespace Vodovoz.Core.Data.NHibernate.BasicHandbooks
{
	public class EquipmentKindMap : ClassMap<EquipmentKind>
	{
		public EquipmentKindMap()
		{
			Table("equipment_kind");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name");

			Map(x => x.WarrantyCardType)
				.Column("warranty_card_type");

			Map(x => x.EquipmentType)
				.Column("equipment_type");
		}
	}
}
