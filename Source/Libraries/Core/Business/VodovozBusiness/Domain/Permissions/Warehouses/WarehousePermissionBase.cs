using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Права на склад",
		Nominative = "Право на склад")]
	[EntityPermission]
	public abstract class WarehousePermissionBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private PermissionType _permissionType;

		[Display(Name = "Чьи права")]
		public virtual PermissionType PermissionType
		{
			get => _permissionType;
			set => SetField(ref _permissionType, value);
		}

		private WarehousePermissionsType _warehousePermissionType;

		[Display(Name = "Права склада")]
		public virtual WarehousePermissionsType WarehousePermissionType
		{
			get => _warehousePermissionType;
			set => SetField(ref _warehousePermissionType, value);
		}

		private Warehouse _warehouse;

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		private bool? _permissionValue;

		[Display(Name = "Значение")]
		public virtual bool? PermissionValue
		{
			get => _permissionValue;
			set => SetField(ref _permissionValue, value);
		}
	}

	public enum PermissionType
	{
		User,
		Subdivision
	}
}
