using FluentNHibernate.Mapping;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.HibernateMapping.Users
{
	public class SpecialPrivilegeMap : SubclassMap<SpecialPrivilege>
	{
		public SpecialPrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.SpecialPrivilege));
		}
	}
}
