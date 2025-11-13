using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Users
{
	public class TablePrivilegeMap : SubclassMap<TablePrivilege>
	{
		public TablePrivilegeMap()
		{
			DiscriminatorValue(nameof(PrivilegeType.TablePrivilege));
		}
	}
}
