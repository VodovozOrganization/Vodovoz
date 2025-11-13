using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions
{
	public class SpecialPrivilege : PrivilegeBase
	{
		public override PrivilegeType PrivilegeType => PrivilegeType.SpecialPrivilege;
		public override string DatabaseName => "mysql";
		public override string TableName => "*";
	}
}
