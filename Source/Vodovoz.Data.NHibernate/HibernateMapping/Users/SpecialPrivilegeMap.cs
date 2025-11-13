using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class SpecialPrivilegeMap : SubclassMap<SpecialPrivilege>
	{
		public SpecialPrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.SpecialPrivilege));
		}
	}
}
