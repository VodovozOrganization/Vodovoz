using System;
using System.Reflection;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Repositories;

namespace Vodovoz.Domain.Permissions
{
	public class EntityPermissionValidator : QS.DomainModel.Entity.EntityPermissions.EntityPermissionValidator
	{
		public override EntityPermission Validate<TEntityType>(int userId)
		{
			return Validate(typeof(TEntityType), userId);
		}

		public override EntityPermission Validate(Type entityType, int userId)
		{
			var permission = base.Validate(entityType, userId);
			if(!permission.IsEmpty) {
				return permission;
			}

			//TODO проверка прав по подразделениям
			return permission;
		}
	}
}
