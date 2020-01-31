using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class PresetUserPermissionMap : SubclassMap<HierarchicalPresetUserPermission>
	{
		public PresetUserPermissionMap()
		{
			DiscriminatorValue(PresetPermissionType.user.ToString());

			References(x => x.User).Column("user_id");
		}
	}
}
