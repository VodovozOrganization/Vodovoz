using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    [EntityPermission]
    public class SubdivisionWarehousePermission: WarehousePermission, IDomainObject
    {
        public override PermissionType PermissionType => PermissionType.Subdivision;

        private Subdivision subdivision;
        [Display(Name = "Подразделение")]
        public override Subdivision Subdivision {
            get => subdivision;
            set => SetField(ref subdivision, value);
        }
    }
}