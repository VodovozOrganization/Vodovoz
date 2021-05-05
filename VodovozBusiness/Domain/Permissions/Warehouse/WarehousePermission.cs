using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouse
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Права на склад",
		Nominative = "Право на склад")]
	[EntityPermission]
    public abstract class WarehousePermission : PropertyChangedBase, IDomainObject
    {
	    public virtual int Id { get; set; }
	    
	    private TypePermissions typePermissions;
        [Display(Name = "Чьи права")]
        public virtual TypePermissions TypePermissions {
            get => typePermissions;
            set => SetField(ref typePermissions, value);
        }
        
        private WarehousePermissions warehousePermissionType;
		[Display(Name = "Права склада")]
        public virtual WarehousePermissions WarehousePermissionType
        {
	        get => warehousePermissionType;
	        set => SetField(ref warehousePermissionType, value);
        }

        private Store.Warehouse warehouse;
        [Display(Name = "Склад")]
        public virtual Store.Warehouse Warehouse
        {
	        get => warehouse;
	        set => SetField(ref warehouse, value);
        }

        private bool? valuePermission;
        [Display(Name = "Значение")]
        public virtual bool? ValuePermission
        {
	        get => valuePermission;
	        set => SetField(ref valuePermission, value);
        }

        private User user;
        [Display(Name = "Пользователь")]
        public virtual User User
        {
	        get => user;
	        set => SetField(ref user, value);
        }
        
        private Subdivision subdivision;
        [Display(Name = "Подразделение")]
        public virtual Subdivision Subdivision
        {
	        get => subdivision;
	        set => SetField(ref subdivision, value);
        }
    }
    
    public enum TypePermissions
    {
        User,
        Subdivision
    }
    
    public enum WarehousePermissions
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