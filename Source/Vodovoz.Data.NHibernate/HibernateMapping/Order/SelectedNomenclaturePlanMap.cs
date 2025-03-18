using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class SelectedNomenclaturePlanMap : ClassMap<SelectedNomenclaturePlan>
	{
		public SelectedNomenclaturePlanMap()
		{
			Table("selected_nomenclature_plan");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
		}


		public class SelectedNomenclatureMap : SubclassMap<SelectedNomenclature>
		{
			public SelectedNomenclatureMap()
			{
				DiscriminatorValue("Nomenclature");

				References(x => x.Nomenclature).Column("nomenclature_id");
			}
		}

		public class SelectedEquipmentKindMap : SubclassMap<SelectedEquipmentKind>
		{
			public SelectedEquipmentKindMap()
			{
				DiscriminatorValue("EquipmentKind");

				References(x => x.EquipmentKind).Column("equipment_kind_id");
			}
		}

		public class SelectedEquipmentTypeMap : SubclassMap<SelectedEquipmentType>
		{
			public SelectedEquipmentTypeMap()
			{
				DiscriminatorValue("EquipmentType");

				Map(x => x.EquipmentType).Column("equipment_type");
			}
		}

		public class SelectedProceedsMap : SubclassMap<SelectedProceeds>
		{
			public SelectedProceedsMap()
			{
				DiscriminatorValue("Proceeds");

				Map(x => x.InludeProceeds).Column("include_proceeds");
			}
		}
	}
}
