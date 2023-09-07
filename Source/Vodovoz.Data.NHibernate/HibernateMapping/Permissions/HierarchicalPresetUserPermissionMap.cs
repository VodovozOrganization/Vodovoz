using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Permissions
{
	public class HierarchicalPresetUserPermissionMap : SubclassMap<HierarchicalPresetUserPermission>
	{
		public HierarchicalPresetUserPermissionMap()
		{
			DiscriminatorValue(PresetPermissionType.user.ToString());

			References(x => x.User).Column("user_id");
		}
	}
}
