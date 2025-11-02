using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Право на склад.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "право на склад",
		AccusativePlural = "права на склад",
		Genitive = "права на склад",
		GenitivePlural = "прав складов",
		Nominative = "Право на склад",
		NominativePlural = "Права на склад",
		Prepositional = "праве на склад",
		PrepositionalPlural = "правах на склады")]
	[EntityPermission]
	public abstract class WarehousePermissionBase : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private PermissionType _permissionType;
		private WarehousePermissionsType _warehousePermissionType;
		private Warehouse _warehouse;
		private bool? _permissionValue;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Чьи права
		/// </summary>
		[Display(Name = "Чьи права")]
		public virtual PermissionType PermissionType
		{
			get => _permissionType;
			set => SetField(ref _permissionType, value);
		}

		/// <summary>
		/// Права склада
		/// </summary>
		[Display(Name = "Права склада")]
		public virtual WarehousePermissionsType WarehousePermissionType
		{
			get => _warehousePermissionType;
			set => SetField(ref _warehousePermissionType, value);
		}

		/// <summary>
		/// Склад
		/// </summary>
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		/// <summary>
		/// Значение
		/// </summary>
		[Display(Name = "Значение")]
		public virtual bool? PermissionValue
		{
			get => _permissionValue;
			set => SetField(ref _permissionValue, value);
		}
	}
}
