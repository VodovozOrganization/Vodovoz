namespace Vodovoz.EntityRepositories.Permissions
{
	public class UserEntityExtendedPermissionNode : UserNode
	{
		public bool CanRead { get; set; }
		public bool CanCreate { get; set; }
		public bool CanUpdate { get; set; }
		public bool CanDelete { get; set; }
		public bool? ExtendedPermissionValue { get; set; }
	}
}
