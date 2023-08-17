using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class DatabasePrivilegeMap : SubclassMap<DatabasePrivilege>
	{
		public DatabasePrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.DatabasePrivilege));
		}
	}
}
