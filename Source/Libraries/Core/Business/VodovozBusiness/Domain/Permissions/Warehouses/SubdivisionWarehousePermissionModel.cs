using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public class SubdivisionWarehousePermissionModel : WarehousePermissionModelBase
	{
		private IUnitOfWork _uow;
		private Subdivision _subdivision;

		public SubdivisionWarehousePermissionModel(IUnitOfWork unitOfWork, Subdivision subdivision)
		{
			_uow = unitOfWork;
			_subdivision = subdivision;
			AllPermission = _uow.Session
				.QueryOver<SubdivisionWarehousePermission>().Where(x => x.Subdivision.Id == _subdivision.Id)
				.List().ToList<WarehousePermissionBase>();
		}

		public override void AddOnUpdatePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse, bool? permissionValue)
		{
			var findPermission = AllPermission.SingleOrDefault(x =>
				x.Warehouse == warehouse &&
				x.WarehousePermissionType == warehousePermissionType);
			if(findPermission is null)
			{
				var subdivisionWarehousePermission = new SubdivisionWarehousePermission
				{
					Subdivision = _subdivision,
					PermissionType = PermissionType.Subdivision,
					Warehouse = warehouse,
					PermissionValue = permissionValue,
					WarehousePermissionType = warehousePermissionType
				};
				_uow.Save(subdivisionWarehousePermission);
			}
			else
			{
				findPermission.PermissionValue = permissionValue;
				_uow.Save(findPermission);
			}
		}

		public override void DeletePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse)
		{
			var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionType == warehousePermissionType);
			if(permissionForDelete != null)
			{
				_uow.Delete(permissionForDelete);
			}
		}

		public override IList<WarehousePermissionBase> AllPermission { get; set; }
	}
}
