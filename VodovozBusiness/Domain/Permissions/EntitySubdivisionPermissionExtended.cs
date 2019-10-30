using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;

namespace Vodovoz.Domain.Permissions
{
	public class EntitySubdivisionPermissionExtended : EntityPermissionExtendedBase
	{
		public override PermissionExtendedType PermissionExtendedType => PermissionExtendedType.Subdivision;

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

	}
}
