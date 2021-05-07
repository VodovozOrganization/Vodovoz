using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    [EntityPermission]
    public class UserWarehousePermission : WarehousePermission, IDomainObject
    {
        public override TypePermissions TypePermissions => TypePermissions.User;
        
        private User user;
        [Display(Name = "Пользователь")]
        public override User User {
            get => user;
            set => SetField(ref user, value);
        }
    }
}