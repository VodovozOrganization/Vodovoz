using FluentNHibernate.Mapping;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Permissions
{
	public class EntitySubdivisionPermissionExtendedMap : SubclassMap<EntitySubdivisionPermissionExtended>
	{
		public EntitySubdivisionPermissionExtendedMap()
		{
			DiscriminatorValue(PermissionExtendedType.Subdivision.ToString());
			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
