using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Documents;
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

	public enum WarehousePermissionsType
	{
		[Display(Name = "Просмотр склада")]
		WarehouseView,

		[Display(Name = "Архивирование склада")]
		Archive,

		[Display(Name = "Изменение талона погрузки")]
		[DocumentType(DocumentType.CarLoadDocument)]
		CarLoadEdit,

		[Display(Name = "Изменение талона разгрузки")]
		[DocumentType(DocumentType.CarUnloadDocument)]
		CarUnloadEdit,

		[Display(Name = "Создание входящей накладной")]
		[DocumentType(DocumentType.IncomingInvoice)]
		IncomingInvoiceCreate,

		[Display(Name = "Изменение входящей накладной")]
		[DocumentType(DocumentType.IncomingInvoice)]
		IncomingInvoiceEdit,

		[Display(Name = "Изменение документа производства")]
		[DocumentType(DocumentType.IncomingWater)]
		IncomingWaterEdit,

		[Display(Name = "Изменение инвентаризации")]
		[DocumentType(DocumentType.InventoryDocument)]
		InventoryEdit,

		[Display(Name = "Создание акта передачи склада")]
		[DocumentType(DocumentType.ShiftChangeDocument)]
		ShiftChangeCreate,

		[Display(Name = "Изменение акта передачи склада")]
		[DocumentType(DocumentType.ShiftChangeDocument)]
		ShiftChangeEdit,

		[Display(Name = "Изменение перемещения")]
		[DocumentType(DocumentType.MovementDocument)]
		MovementEdit,

		[Display(Name = "Изменение пересортицы")]
		[DocumentType(DocumentType.RegradingOfGoodsDocument)]
		RegradingOfGoodsEdit,

		[Display(Name = "Изменение отпуск самовывоза")]
		[DocumentType(DocumentType.SelfDeliveryDocument)]
		SelfDeliveryEdit,

		[Display(Name = "Изменение акта списания")]
		[DocumentType(DocumentType.WriteoffDocument)]
		WriteoffEdit
	}

	public class DocumentTypeAttribute : Attribute
	{
		public DocumentType Type { get; set; }

		public DocumentTypeAttribute(DocumentType type)
		{
			Type = type;
		}
	}
}
