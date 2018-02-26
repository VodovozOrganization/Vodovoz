using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Tools.Permissions
{
	public enum WarehousePermissions
	{
		[Display(Name = "Изменение талона погрузки")]
		CarLoadEdit,
		[Display(Name = "Изменение талона разгрузки")]
		CarUnloadEdit,
		[Display(Name = "Изменение входящей накладной")]
		IncomingInvoiceEdit,
		[Display(Name = "Изменение документа производства")]
		IncomingWaterEdit,
		[Display(Name = "Изменение инвентаризации")]
		InventoryEdit,
		[Display(Name = "Изменение перемещения")]
		MovementEdit,
		[Display(Name = "Изменение пересортицы")]
		RegradingOfGoodsEdit,
		[Display(Name = "Изменение отпуск самовывоза")]
		SelfDeliveryEdit,
		[Display(Name = "Изменение акта списания")]
		WriteoffEdit
	}
}
