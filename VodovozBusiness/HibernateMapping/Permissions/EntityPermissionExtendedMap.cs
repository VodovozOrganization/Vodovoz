using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class EntityPermissionExtendedMap : ClassMap<EntityPermissionExtended>
	{
		public EntityPermissionExtendedMap()
		{
			Table("entity_permission_extended");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PermissionId).Column("permission_id");

			Map(x => x.IsPermissionAvailable).Column("is_permission_available");

			References(x => x.User).Column("user_id");
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.TypeOfEntity).Column("type_of_entity_id");
		}
	}
}
