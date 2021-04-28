using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class SubdivisionWarehousePermissionModel : WarehousePermissionModel
    {
        private IUnitOfWork unitOfWork;
        private Subdivision subdivision;
        public SubdivisionWarehousePermissionModel(IUnitOfWork unitOfWork, Subdivision subdivision)
        {
            this.unitOfWork = unitOfWork;
            this.subdivision = subdivision;
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
                    Subdivision = subdivision,
                    TypePermissions = TypePermissions.Subdivision,
                    Warehouse = warehouse,
                    ValuePermission = permissionValue,
                    WarehousePermissionType = warehousePermission
                };
                unitOfWork.Save(subdivisionWarehousePermission);
            }
            else
            {
                findPermission.ValuePermission = permissionValue;
                unitOfWork.Save(findPermission);
            }
        }

        public override void DeletePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse)
        {
            var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionType == warehousePermission);
            if (permissionForDelete != null)
                unitOfWork.TryDelete(permissionForDelete);
        }

        public override IEnumerable<WarehousePermission> GetEnumerator()
        {
            var query = unitOfWork.Session.QueryOver<SubdivisionWarehousePermission>().List();

            return query?.Where(x => x.Subdivision.Id == subdivision.Id);
        }

        public override List<WarehousePermission> AllPermission { get; set; }
    }
}