using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    [EntityPermission]
    public class UserWarehousePermission : WarehousePermission, IDomainObject
    {
        public override PermissionType PermissionType => PermissionType.User;
        
        private User user;
        [Display(Name = "Пользователь")]
        public override User User {
            get => user;
            set => SetField(ref user, value);
        }
    }
}