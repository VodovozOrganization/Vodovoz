using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class SubdivisionWarehousePermissionModel : WarehousePermissionModel
    {
        private IUnitOfWork _uow;
        private Subdivision _subdivision;
        public SubdivisionWarehousePermissionModel(IUnitOfWork unitOfWork, Subdivision subdivision)
        {
            this._uow = unitOfWork;
            this._subdivision = subdivision;
            AllPermission = GetEnumerator().ToList();
        }

        public override void AddOnUpdatePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse, bool? permissionValue)
        {
            var findPermission = AllPermission.SingleOrDefault(x =>
                x.Warehouse == warehouse &&
                x.WarehousePermissionType == warehousePermission);
            if (findPermission is null)
            {
                var subdivisionWarehousePermission = new SubdivisionWarehousePermission
                {
                    Subdivision = _subdivision,
                    TypePermissions = TypePermissions.Subdivision,
                    Warehouse = warehouse,
                    ValuePermission = permissionValue,
                    WarehousePermissionType = warehousePermission
                };
                _uow.Save(subdivisionWarehousePermission);
            }
            else
            {
                findPermission.ValuePermission = permissionValue;
                _uow.Save(findPermission);
            }
        }

        public override void DeletePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse)
        {
            var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionType == warehousePermission);
            if (permissionForDelete != null)
                _uow.TryDelete(permissionForDelete);
        }

        public override IEnumerable<WarehousePermission> GetEnumerator() => _uow.Session
            .QueryOver<SubdivisionWarehousePermission>().Where(x => x.Subdivision.Id == _subdivision.Id)
            .List();

        public override List<WarehousePermission> AllPermission { get; set; }
    }
}