using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	[EntityPermission]
	[HistoryTrace]
	public class SubdivisionWarehousePermission: WarehousePermissionBase
	{
		public override PermissionType PermissionType => PermissionType.Subdivision;

		private Subdivision _subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}
	}
}
