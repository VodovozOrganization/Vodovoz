using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions
{
	public class GlobalPrivilege : PrivilegeBase
	{
		public override PrivilegeType PrivilegeType => PrivilegeType.GlobalPrivilege;
		public override string DatabaseName => "*";
		public override string TableName => "*";
	}
}
