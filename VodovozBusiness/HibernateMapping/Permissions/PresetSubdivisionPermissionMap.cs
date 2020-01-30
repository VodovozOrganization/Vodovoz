using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class PresetSubdivisionPermissionMap : SubclassMap<PresetSubdivisionPermission>
	{
		public PresetSubdivisionPermissionMap()
		{
			DiscriminatorValue(PresetPermissionType.subdivision.ToString());

			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
