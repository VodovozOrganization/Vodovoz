using System;
using NHibernate.Mapping;
using Vodovoz.Domain.Permissions;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping.Permissions
{
	public class EntitySubdivisionPermissionMap //: ClassMap<EntitySubdivisionPermission>
	{
		/*public EntitySubdivisionPermissionMap()
		{
			Table("permission_entity_subdivision");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.EntityName).Column("entity_name");
			Map(x => x.CanCreate).Column("can_create");
			Map(x => x.CanRead).Column("can_read");
			Map(x => x.CanUpdate).Column("can_update");
			Map(x => x.CanDelete).Column("can_delete");

			References(x => x.Subdivision).Column("subdivision_id");
		}*/
	}
}
