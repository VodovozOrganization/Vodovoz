using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;
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
