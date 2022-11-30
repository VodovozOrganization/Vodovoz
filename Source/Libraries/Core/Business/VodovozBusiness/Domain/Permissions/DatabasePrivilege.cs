namespace Vodovoz.Domain.Permissions
{
	public class DatabasePrivilege : PrivilegeBase
	{
		public override PrivilegeType PrivilegeType => PrivilegeType.DatabasePrivilege;
		public override string TableName => "*";
	}
}
