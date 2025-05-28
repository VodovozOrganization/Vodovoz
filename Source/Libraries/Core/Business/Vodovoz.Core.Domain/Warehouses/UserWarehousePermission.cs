using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	[EntityPermission]
	[HistoryTrace]
	public class UserWarehousePermission : WarehousePermissionBase
	{
		public override PermissionType PermissionType => PermissionType.User;

		private User _user;
		[Display(Name = "Пользователь")]
		public virtual User User {
			get => _user;
			set => SetField(ref _user, value);
		}
	}
}
