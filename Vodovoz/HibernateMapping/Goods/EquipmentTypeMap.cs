using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class EquipmentTypeMap : ClassMap<EquipmentType>
	{
		public EquipmentTypeMap ()
		{
			Table ("equipment_type");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}

