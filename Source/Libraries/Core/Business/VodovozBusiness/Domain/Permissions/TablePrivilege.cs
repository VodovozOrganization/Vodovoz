using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions
{
	public class TablePrivilege : PrivilegeBase
	{
		public override PrivilegeType PrivilegeType => PrivilegeType.TablePrivilege;
	}
}
