using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class UserWarehousePermission : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private User user;
        [Display(Name = "Пользователь")]
        public virtual User User {
            get => user;
            set => SetField(ref user, value);
        }
    }
}