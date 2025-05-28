using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Warehouses.Documents
{
	/// <summary>
	/// Тип документа
	/// </summary>
	public enum DocumentType
	{
		/// <summary>
		/// Входящая накладная
		/// </summary>
		[Display(Name = "Входящая накладная")]
		IncomingInvoice,
		/// <summary>
		/// Документ производства
		/// </summary>
		[Display(Name = "Документ производства")]
		IncomingWater,
		/// <summary>
		/// Документ перемещения
		/// </summary>
		[Display(Name = "Документ перемещения")]
		MovementDocument,
		/// <summary>
		/// Акт списания
		/// </summary>
		[Display(Name = "Акт списания")]
		WriteoffDocument,
		/// <summary>
		/// Отпуск самовывоза
		/// </summary>
		[Display(Name = "Отпуск самовывоза")]
		SelfDeliveryDocument,
		/// <summary>
		/// Талон погрузки
		/// </summary>
		[Display(Name = "Талон погрузки")]
		CarLoadDocument,
		/// <summary>
		/// Талон разгрузки
		/// </summary>
		[Display(Name = "Талон разгрузки")]
		CarUnloadDocument,
		/// <summary>
		/// Инвентаризация
		/// </summary>
		[Display(Name = "Инвентаризация")]
		InventoryDocument,
		/// <summary>
		/// Акт передачи остатков
		/// </summary>
		[Display(Name = "Акт передачи остатков")]
		ShiftChangeDocument,
		/// <summary>
		/// Пересортица товаров
		/// </summary>
		[Display(Name = "Пересортица товаров")]
		RegradingOfGoodsDocument,
		/// <summary>
		/// Документ доставки
		/// </summary>
		[Display(Name = "Документ доставки")]
		DeliveryDocument,
		/// <summary>
		/// Документы перемещения терминала водителя
		/// </summary>
		[Display(Name = "Документы перемещения терминала водителя")]
		DriverTerminalMovement,
		/// <summary>
		/// Документ выдачи терминала водителя
		/// </summary>
		[Display(Name = "Документ выдачи терминала водителя")]
		DriverTerminalGiveout,
		/// <summary>
		/// Документ возврата терминала водителя
		/// </summary>
		[Display(Name = "Документ возврата терминала водителя")]
		DriverTerminalReturn
	}
}
