using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class EquipmentColorsMap : ClassMap<EquipmentColors>
	{
		public EquipmentColorsMap ()
		{
			Table ("equipment_colors");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}

