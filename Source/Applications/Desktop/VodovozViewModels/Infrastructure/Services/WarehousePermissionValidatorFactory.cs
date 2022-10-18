using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.ViewModels.Infrastructure.Services
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
