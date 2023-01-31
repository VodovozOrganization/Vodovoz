namespace Vodovoz.EntityRepositories.Permissions
{
	public class UserEntityExtendedPermissionWithSubdivisionNode : UserWithSubdivisionNode
	{
		public bool? CanRead { get; set; }
		public bool? CanCreate { get; set; }
		public bool? CanUpdate { get; set; }
		public bool? CanDelete { get; set; }
		public bool? ExtendedPermissionValue { get; set; }
		public bool HasPermission => CanRead.HasValue && CanCreate.HasValue && CanUpdate.HasValue && CanDelete.HasValue;
	}
}
