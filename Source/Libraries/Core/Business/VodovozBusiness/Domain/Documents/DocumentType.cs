using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents
{
	public enum DocumentType
	{
		[Display(Name = "Входящая накладная")]
		IncomingInvoice,
		[Display(Name = "Документ производства")]
		IncomingWater,
		[Display(Name = "Документ перемещения")]
		MovementDocument,
		[Display(Name = "Акт списания")]
		WriteoffDocument,
		[Display(Name = "Отпуск самовывоза")]
		SelfDeliveryDocument,
		[Display(Name = "Талон погрузки")]
		CarLoadDocument,
		[Display(Name = "Талон разгрузки")]
		CarUnloadDocument,
		[Display(Name = "Инвентаризация")]
		InventoryDocument,
		[Display(Name = "Акт передачи остатков")]
		ShiftChangeDocument,
		[Display (Name = "Пересортица товаров")]
		RegradingOfGoodsDocument,
		[Display (Name = "Документ доставки")]
		DeliveryDocument,
		[Display(Name = "Документы перемещения терминала водителя")]
		DriverTerminalMovement,
		[Display(Name = "Документ выдачи терминала водителя")]
		DriverTerminalGiveout,
		[Display(Name = "Документ возврата терминала водителя")]
		DriverTerminalReturn
	}
}

