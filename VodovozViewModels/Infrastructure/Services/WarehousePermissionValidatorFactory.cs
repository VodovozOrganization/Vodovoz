using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;
namespace Vodovoz.Core
{
	public class WarehousePermissionValidatorFactory : IWarehousePermissionValidatorFactory
	{
		public IWarehousePermissionValidator CreateValidator(Subdivision subdivision)
		{
			List<SubdivisionWarehousePermission> warehousePermission = new List<SubdivisionWarehousePermission>();
			do
			{
				using (var uow = UnitOfWorkFactory.CreateForRoot<Subdivision>(subdivision.Id))
					warehousePermission.AddRange(uow.Session.QueryOver<SubdivisionWarehousePermission>()
						.Where(x => x.Subdivision.Id == subdivision.Id).List());
				subdivision = subdivision.ParentSubdivision;
			} while (subdivision != null);
			return new WarehousePermissionValidator(warehousePermission);
		}
	}
}
