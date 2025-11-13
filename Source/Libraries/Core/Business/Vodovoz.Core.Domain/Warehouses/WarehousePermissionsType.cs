using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Warehouses.Documents;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Типы прав на склад.
	/// </summary>
	public enum WarehousePermissionsType
	{
		/// <summary>
		/// Просмотр склада
		/// </summary>
		[Display(Name = "Просмотр склада")]
		WarehouseView,

		/// <summary>
		/// Архивирование склада
		/// </summary>
		[Display(Name = "Архивирование склада")]
		Archive,

		/// <summary>
		/// Изменение талона погрузки
		/// </summary>
		[Display(Name = "Изменение талона погрузки")]
		[DocumentType(DocumentType.CarLoadDocument)]
		CarLoadEdit,

		/// <summary>
		/// Изменение талона разгрузки
		/// </summary>
		[Display(Name = "Изменение талона разгрузки")]
		[DocumentType(DocumentType.CarUnloadDocument)]
		CarUnloadEdit,

		/// <summary>
		/// Создание входящей накладной
		/// </summary>
		[Display(Name = "Создание входящей накладной")]
		[DocumentType(DocumentType.IncomingInvoice)]
		IncomingInvoiceCreate,

		/// <summary>
		/// Изменение входящей накладной
		/// </summary>
		[Display(Name = "Изменение входящей накладной")]
		[DocumentType(DocumentType.IncomingInvoice)]
		IncomingInvoiceEdit,

		/// <summary>
		/// Изменение документа производства
		/// </summary>
		[Display(Name = "Изменение документа производства")]
		[DocumentType(DocumentType.IncomingWater)]
		IncomingWaterEdit,

		/// <summary>
		/// Изменение инвентаризации
		/// </summary>
		[Display(Name = "Изменение инвентаризации")]
		[DocumentType(DocumentType.InventoryDocument)]
		InventoryEdit,

		/// <summary>
		/// Создание акта передачи склада
		/// </summary>
		[Display(Name = "Создание акта передачи склада")]
		[DocumentType(DocumentType.ShiftChangeDocument)]
		ShiftChangeCreate,

		/// <summary>
		/// Изменение акта передачи склада
		/// </summary>
		[Display(Name = "Изменение акта передачи склада")]
		[DocumentType(DocumentType.ShiftChangeDocument)]
		ShiftChangeEdit,

		/// <summary>
		/// Изменение перемещения
		/// </summary>
		[Display(Name = "Изменение перемещения")]
		[DocumentType(DocumentType.MovementDocument)]
		MovementEdit,

		/// <summary>
		/// Изменение пересортицы
		/// </summary>
		[Display(Name = "Изменение пересортицы")]
		[DocumentType(DocumentType.RegradingOfGoodsDocument)]
		RegradingOfGoodsEdit,

		/// <summary>
		/// Изменение отпуск самовывоза
		/// </summary>
		[Display(Name = "Изменение отпуск самовывоза")]
		[DocumentType(DocumentType.SelfDeliveryDocument)]
		SelfDeliveryEdit,

		/// <summary>
		/// Изменение акта списания
		/// </summary>
		[Display(Name = "Изменение акта списания")]
		[DocumentType(DocumentType.WriteoffDocument)]
		WriteoffEdit
	}
}
