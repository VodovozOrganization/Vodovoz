using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Users
{
	public class GlobalPrivilegeMap : SubclassMap<GlobalPrivilege>
	{
		public GlobalPrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.GlobalPrivilege));
		}
	}
}
