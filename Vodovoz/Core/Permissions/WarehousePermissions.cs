using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Core.Permissions
{
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
		[Display(Name = "Изменение входящей накладной")]
		[DocumentType(DocumentType.IncomingInvoice)]
		IncomingInvoiceEdit,
		[Display(Name = "Изменение документа производства")]
		[DocumentType(DocumentType.IncomingWater)]
		IncomingWaterEdit,
		[Display(Name = "Изменение инвентаризации")]
		[DocumentType(DocumentType.InventoryDocument)]
		InventoryEdit,
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

	[AttributeUsage(AttributeTargets.Field)]
	public class DocumentTypeAttribute : Attribute
	{
		public DocumentType Type { get; set; }

		public DocumentTypeAttribute(DocumentType type)
		{
			Type = type;
		}
	}

}
