using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class HierarchicalPresetSubdivisionPermissionMap : SubclassMap<HierarchicalPresetSubdivisionPermission>
	{
		public HierarchicalPresetSubdivisionPermissionMap()
		{
			DiscriminatorValue(PresetPermissionType.subdivision.ToString());

			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
