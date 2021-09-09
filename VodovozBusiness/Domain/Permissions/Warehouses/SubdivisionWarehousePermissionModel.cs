using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    public class SubdivisionWarehousePermissionModelBase : WarehousePermissionModelBase
    {
        private IUnitOfWork _uow;
        private Subdivision _subdivision;
        public SubdivisionWarehousePermissionModelBase(IUnitOfWork unitOfWork, Subdivision subdivision)
        {
            this._uow = unitOfWork;
            this._subdivision = subdivision;
            AllPermission = GetEnumerator().ToList();
        }

        public override void AddOnUpdatePermission(WarehousePermissionsType warehousePermissionType, Store.Warehouse warehouse, bool? permissionValue)
        {
            var findPermission = AllPermission.SingleOrDefault(x =>
                x.Warehouse == warehouse &&
                x.WarehousePermissionTypeType == warehousePermissionType);
            if (findPermission is null)
            {
                var subdivisionWarehousePermission = new SubdivisionWarehousePermission
                {
                    Subdivision = _subdivision,
                    PermissionType = PermissionType.Subdivision,
                    Warehouse = warehouse,
                    PermissionValue = permissionValue,
                    WarehousePermissionTypeType = warehousePermissionType
                };
                _uow.Save(subdivisionWarehousePermission);
            }
            else
            {
                findPermission.PermissionValue = permissionValue;
                _uow.Save(findPermission);
            }
        }

        public override void DeletePermission(WarehousePermissionsType warehousePermissionType, Store.Warehouse warehouse)
        {
            var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionTypeType == warehousePermissionType);
            if (permissionForDelete != null)
                _uow.TryDelete(permissionForDelete);
        }

        public override IEnumerable<WarehousePermissionBase> GetEnumerator() => _uow.Session
            .QueryOver<SubdivisionWarehousePermission>().Where(x => x.Subdivision.Id == _subdivision.Id)
            .List();

        public override List<WarehousePermissionBase> AllPermission { get; set; }
    }
}
