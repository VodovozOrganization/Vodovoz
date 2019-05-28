using System;
using QS.DomainModel.Entity.EntityPermissions;
namespace Vodovoz.Infrastructure.Services
{
	public class PermissionService : IPermissionService
	{
		private readonly IEntityPermissionValidator permissionValidator;

		public PermissionService(IEntityPermissionValidator permissionValidator)
		{
			this.permissionValidator = permissionValidator ?? throw new ArgumentNullException(nameof(permissionValidator));
		}

		public IPermissionResult ValidateUserPermission(Type entityType, int userId)
		{
			EntityPermission permission = permissionValidator.Validate(entityType, userId);
			return new PermissionResult(permission);
		}
	}

	public class PermissionResult : IPermissionResult
	{
		private readonly EntityPermission entityPermission;

		public bool CanCreate => entityPermission.Create;
		public bool CanRead => entityPermission.Read;
		public bool CanUpdate => entityPermission.Update;
		public bool CanDelete => entityPermission.Delete;

		public PermissionResult(EntityPermission entityPermission)
		{
			this.entityPermission = entityPermission;
		}
	}
}
