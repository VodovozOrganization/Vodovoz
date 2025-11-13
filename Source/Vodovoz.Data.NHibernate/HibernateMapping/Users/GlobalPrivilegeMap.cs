using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class GlobalPrivilegeMap : SubclassMap<GlobalPrivilege>
	{
		public GlobalPrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.GlobalPrivilege));
		}
	}
}
